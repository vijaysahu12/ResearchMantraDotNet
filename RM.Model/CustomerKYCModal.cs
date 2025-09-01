using System.ComponentModel.DataAnnotations;

namespace RM.Model
{
    public class CustomerKYCModal
    {
        public string Occupation { get; set; }
        public string age_group { get; set; }
        public string all_emi { get; set; }
        public string annual_income { get; set; }
        public string commodity_investment { get; set; }
        public string duration_of_investment { get; set; }
        public string email_id { get; set; }
        public string emergency_fund { get; set; }
        public string equity_investment { get; set; }
        public string financially_supports { get; set; }
        public string forex_investment { get; set; }
        public string id_no { get; set; }
        public string investment_amount { get; set; }
        public string investment_experience { get; set; }
        public string investment_goal { get; set; }
        public string investment_in_past { get; set; }
        public string mobile { get; set; }
        public string Name { get; set; }
        public string note { get; set; }
        public string option_1 { get; set; }
        public string option_2 { get; set; }
        public string option_3 { get; set; }
        public string option_5 { get; set; }
        public string primary_income { get; set; }
        public string related { get; set; }
        public string risk_category { get; set; }
        public string secondary_sources { get; set; }
        public string service_name { get; set; }
        public string traded_previously { get; set; }
        public KYCFile user_img { get; set; }

        [Required]
        public KYCFile pan_img { get; set; }


        [Required]
        public KYCFile signature_img { get; set; }


        public string value_of_portfolio { get; set; }
        public string pan { get; set; }
    }


    public class KYCFile
    {
        public string filename { get; set; }
        public string filetype { get; set; }
        public string value { get; set; }
    }
}
