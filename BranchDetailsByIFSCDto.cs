
using System;

public class BranchDetailsByIFSCDto
{
    public String BranchId { get; set; }

    public String BankId { get; set; }

    public String BankName { get; set; }

    public String BranchCode { get; set; }

    public String IFSCCode { get; set; }

    public String BranchAddress { get; set; }

    public String State { get; set; }

    public String District { get; set; }

    public String BankStatus { get; set; }
    public String BranchStatus { get; set; }

    public String CreatedDate { get; set; }

    public BranchDetailsByIFSCDto(String BranchId, String BankId, String BankName, String BranchCode, String IFSCCode, String BranchAddress, String State, String District, String BankStatus, String BranchStatus, String CreatedDate)
    {
        this.BranchId = BranchId;
        this.BankId = BankId;
        this.BankName = BankName;
        this.BranchCode = BranchCode;
        this.IFSCCode = IFSCCode;
        this.BranchAddress = BranchAddress;
        this.State = State;
        this.District = District;
        this.BankStatus = BankStatus;
        this.BranchStatus = BranchStatus;
        this.CreatedDate = CreatedDate;
    }
}