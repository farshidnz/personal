using System.Collections.Generic;

namespace Cashrewards3API.Features.Promotion.Model
{
    public class StrapiCampaign
    {
        private string _title;
        public string Title
        {
            get { return Data?[0].Attributes?.Title ?? _title; }
            set { _title = value; }
        }

        private string _description;
        public string Description
        {
            get { return Data?[0].Attributes?.Description ?? _description; }
            set { _description = value; }
        }

        private string _longDescription;
        public string Long_Description
        {
            get { return Data?[0].Attributes?.Long_Description ?? _longDescription; }
            set { _longDescription = value; }
        }

        private StrapiBannerImage _bannerImage = new();
        public StrapiBannerImage Banner_Image
        {
            get { return Data?[0].Attributes?.Banner_Image ?? _bannerImage; }
            set { _bannerImage = value; }
        }

        private List<StrapiCategory> _category = new();
        public List<StrapiCategory> Category
        {
            get { return Data?[0].Attributes?.Category ?? _category; }
            set { _category = value; }
        }

        private StrapiCampaignSection _campaignSection = new();
        public StrapiCampaignSection Campaign_Section
        {
            get { return Data?[0].Attributes?.Campaign_Section ?? _campaignSection; }
            set { _campaignSection = value; }
        }

        private StrapiSearchEngineOptimisation _seo = new();
        public StrapiSearchEngineOptimisation Seo
        {
            get { return Data?[0].Attributes?.Seo ?? _seo; }
            set { _seo = value; }
        }

        private List<StrapiCampaigns> _campaigns = new();
        public List<StrapiCampaigns> Campaigns
        {
            get { return Data?[0].Attributes?.Campaigns ?? _campaigns; }
            set { _campaigns = value; }
        }

        public List<StrapiCampaign> Data { get; set; } = null;
        public StrapiCampaign Attributes { get; set; } = null;
    }

    public class StrapiBannerImage
    {
        public StrapiMedia Desktop { get; set; } = new();
        public StrapiMedia Tablet { get; set; } = new();
        public StrapiMedia Mobile { get; set; } = new();
    }

    public class StrapiSearchEngineOptimisation
    {
        public string Meta_Title { get; set; }
        public string Meta_Description { get; set; }
    }

    public class StrapiCategory
    {
        public string Title { get; set; }
        public List<StrapiCategoryItem> Item { get; set; } = new();
    }

    public class StrapiCategoryItem
    {
        public int? Merchant_Id { get; set; }
        public int? Offer_Id { get; set; }
        public StrapiMedia Background_Image { get; set; } = new();
        public bool? Redirect_Go_Page { get; set; } = false;
        public string Past_Rate { get; set; }
        public string Title { get; set; }
    }

    public class StrapiCampaignSection
    {
        public StrapiMedia Large_Head_Image { get; set; }
        public StrapiMedia Medium_Head_Image { get; set; }
        public StrapiMedia Small_Head_Image { get; set; }
        public string Head_Subtitle { get; set; }

        public static string DefaultHeadImage => "https://s3-ap-southeast-2.amazonaws.com/cashrewards.prod.hub-pages/Images/spacer-10.png";

        public string Order { get; set; }

    }

    public class StrapiCampaigns
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public StrapiMedia Campaign_Image { get; set; }

        public List<StrapiOffers> Offers { get; set; } = new();
    }

    public class StrapiOffers
    {
        public string Offer_Title { get; set; }
        public int? Offer_Id { get; set; }
        public decimal? Price { get; set; }
        public decimal? Cashback { get; set; }
        public StrapiMedia Offer_Image { get; set; }
    }


    public class StrapiMedia
    {
        private string _url;
        public string Url
        {
            get { return Data?.Attributes?.Url ?? _url; }
            set { _url = value; }
        }
        public StrapiMedia Data { get; set; } = null;
        public StrapiMedia Attributes { get; set; } = null;
    }
}
