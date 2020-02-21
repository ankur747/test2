using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Test2
{
    public class BankServiceJobs
    {
        IServiceProvider _serviceProvider;

        public BankServiceJobs(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public BankServiceJobs()
        {
        }

        public void UpdateAllBranchDetails()
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            using BankMasterLocalContext context = scope.ServiceProvider.GetRequiredService<BankMasterLocalContext>();
            var allBranches = context.BankPfmsCopy.Select(b => b).ToList();
            allBranches.ForEach(branch => UpdateBranchDetails(branch, context));
        }

        private void UpdateBranchDetails(BankPfmsCopy branch, BankMasterLocalContext context)
        {
            List<BranchDetailsByIFSCDto> branchList = new List<BranchDetailsByIFSCDto>();
            //GetBankBranchByIFSC
            String xmlBranchStr = $"<BankBranchByIFSC><BankName>{branch.BankName}</BankName><IfscCode>{branch.IfscCode}</IfscCode></BankBranchByIFSC>";
            var xmlBranchEle = GetElement(xmlBranchStr);
            try
            {
                BankService.FileReconcilationMessage_GetBankBranchByIFSC req = new BankService.FileReconcilationMessage_GetBankBranchByIFSC
                {
                    Username = "extsyssvc",
                    Password = "cpsms@321!",
                    ResponseMessage = xmlBranchEle
                };

                BankService.FileReconcilationContractClient cc = new BankService.FileReconcilationContractClient();
                var result = cc.PFMS_GetBankBranchByIFSCAsync(req).GetAwaiter().GetResult();
                String xml = result.ResponseMessage.FirstChild.InnerXml;

                XElement doc = XElement.Parse(xml);

                var response = doc.Elements("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BankBranchByIFSC");
                foreach (var item in response)
                {
                    String BranchId = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BranchId").Value;
                    String BankId = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BankId").Value;
                    String BankName = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BankName").Value;
                    String BranchCode = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BranchCode").Value;
                    String IFSCCode = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}IFSCCode").Value;
                    String BranchAddress = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BranchAddress").Value;
                    String State = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}State").Value;
                    String District = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}District").Value;
                    String BankStatus = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BankStatus").Value;
                    String BranchStatus = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BranchStatus").Value;
                    String CreatedDate = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}CreatedDate").Value;
                    var branchListItem = new BranchDetailsByIFSCDto(BranchId, BankId, BankName, BranchCode, IFSCCode, BranchAddress, State, District, BankStatus, BranchStatus, CreatedDate);
                    branchList.Add(branchListItem);
                }
                //handle error cases

                //For more than one respose cases
                branchList.ForEach(b =>
                {
                    if (b.BranchId == branch.BranchId.ToString())
                    {
                        UpdateIfChanges(b, context);
                    }
                });
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void UpdateIfChanges(BranchDetailsByIFSCDto bpfms, BankMasterLocalContext context)
        {

            var branch = context.BankPfmsCopy.Where(br => br.BranchId.ToString() == bpfms.BranchId).Select(p => p).FirstOrDefault();
            Boolean IsModified = CheckForChanges(bpfms, branch);
            if (IsModified)
            {
                branch.IsActive = (bpfms.BranchStatus == "Active" ? true : false);
                branch.Address = bpfms.BranchAddress;
                branch.IfscCode = bpfms.IFSCCode;
                branch.District = bpfms.District;
                branch.State = bpfms.State;
                branch.BankName = bpfms.BankName;
                branch.BranchName = bpfms.BranchCode;
                branch.IsModified = IsModified;
            }
            //Checked with pfms changes
            branch.IsChecked = true;
            
        }
        private bool CheckForChanges(BranchDetailsByIFSCDto bpfms, BankPfmsCopy branch)
        {
            var noChange = (branch.IfscCode == bpfms.IFSCCode);
            noChange = noChange && (branch.IsActive == (bpfms.BranchStatus == "Active" ? true : false));
            noChange = noChange && (branch.State == bpfms.State);
            noChange = noChange && (branch.District == bpfms.District);
            noChange = noChange && (branch.BranchName == bpfms.BranchCode);
            noChange = noChange && (branch.BankName == bpfms.BankName);
            noChange = noChange && (branch.Address == bpfms.BranchAddress);

            return !noChange;

        }

        private static XmlElement GetElement(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement;
        }

        public void GetNewBranches()
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            using BankMasterLocalContext context = scope.ServiceProvider.GetRequiredService<BankMasterLocalContext>();
            var previousDate = "2020-02-01";
            TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            var date_mmddyyyy = indianTime.ToShortDateString();
            var formatDate = date_mmddyyyy.Split("/");
            formatDate[0] = formatDate[0].Length == 1 ? $"0{formatDate[0]}" : formatDate[0];
            formatDate[1] = formatDate[1].Length == 1 ? $"0{formatDate[1]}" : formatDate[1];
            var currentDate = $"{formatDate[2]}-{formatDate[0]}-{formatDate[1]}";
            List<BranchDetailsByIFSCDto> newBranchList = GetNewBranchesBetweenDates(previousDate,currentDate,context);
            AddNewBranches(newBranchList, context);


        }

        private void AddNewBranches(List<BranchDetailsByIFSCDto> newBranchList, BankMasterLocalContext context)
        {
            List<BankPfmsCopy> branchList = newBranchList.Select(b=> new BankPfmsCopy
            {
                BankName=b.BankName,
                BranchId = int.Parse(b.BranchId),
                BranchName =b.BranchCode,
                State=b.State,
                District=b.District,
                IfscCode=b.IFSCCode,
                Address=b.BranchAddress,
                IsActive=b.BranchStatus=="Active"?true:false
            }).ToList();
            BankPfmsCopy branch = branchList[0];
            branch.IsModified = false;
            branch.IsChecked = false;
            context.Add(branch);
            context.SaveChanges();

        }

        private List<BranchDetailsByIFSCDto> GetNewBranchesBetweenDates(String previousDate, String currentDate, BankMasterLocalContext context)
        {
            List<BranchDetailsByIFSCDto> newBranchList = new List<BranchDetailsByIFSCDto>();
            //GetBankBranchByIFSC using dates
            String xmlBranchStr = $"<BankBranchByIFSC><FromDate>{previousDate}</FromDate><ToDate>{currentDate}</ToDate></BankBranchByIFSC>";
            var xmlBranchEle = GetElement(xmlBranchStr);
            try
            {
                BankService.FileReconcilationMessage_GetBankBranchByIFSC req = new BankService.FileReconcilationMessage_GetBankBranchByIFSC
                {
                    Username = "extsyssvc",
                    Password = "cpsms@321!",
                    ResponseMessage = xmlBranchEle
                };

                BankService.FileReconcilationContractClient cc = new BankService.FileReconcilationContractClient();
                var result = cc.PFMS_GetBankBranchByIFSCAsync(req).GetAwaiter().GetResult();
                String xml = result.ResponseMessage.FirstChild.InnerXml;

                XElement doc = XElement.Parse(xml);

                var newBranches = doc.Elements("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BankBranchByIFSC");
                foreach (var item in newBranches)
                {
                    String BranchId = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BranchId").Value;
                    String BankId = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BankId").Value;
                    String BankName = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BankName").Value;
                    String BranchCode = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BranchCode").Value;
                    String IFSCCode = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}IFSCCode").Value;
                    String BranchAddress = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BranchAddress").Value;
                    String State = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}State").Value;
                    String District = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}District").Value;
                    String BankStatus = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BankStatus").Value;
                    String BranchStatus = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}BranchStatus").Value;
                    String CreatedDate = item.Element("{http://webservices.pfms.nic.in/PFMSExternalWebService.xsd}CreatedDate").Value;
                    var branchListItem = new BranchDetailsByIFSCDto(BranchId, BankId, BankName, BranchCode, IFSCCode, BranchAddress, State, District, BankStatus, BranchStatus, CreatedDate);
                    newBranchList.Add(branchListItem);
                }
                //handle case where error returned
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            var branchIdPresentInBankPfmsCopy = context.BankPfmsCopy.Select(b => b.Id).ToList();
            //need to return branches not in table 
            var filterdListOfNewBranches = newBranchList.Where(nb => !branchIdPresentInBankPfmsCopy.Contains(int.Parse(nb.BranchId))).Select(nb => nb).ToList();

            return filterdListOfNewBranches;
        }
    }
}
