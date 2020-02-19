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
            var anyChange = branch.IfscCode == bpfms.IFSCCode ?
                branch.IsActive == (bpfms.BranchStatus == "Active" ? true : false) ?
                branch.State == bpfms.State ?
                branch.District == bpfms.District ?
                branch.BranchName == bpfms.BranchCode ?
                branch.BankName == bpfms.BankName ?
                branch.Address == bpfms.BranchAddress ? false : true : true : true : true : true : true : true;

            return anyChange;

        }

        private static XmlElement GetElement(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement;
        }
    }
}
