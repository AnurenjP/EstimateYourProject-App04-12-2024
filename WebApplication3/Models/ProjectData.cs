﻿namespace ProjectEstimationApp.Models
{
    public class ProjectData
    {
        public string ProjectName { get; set; } = string.Empty; // Add this line

        public List<Resource> Resources { get; set; } = new List<Resource>();
        public string? ProjectStartDate { get; set; }
        public string? ProjectEndDate { get; set; }
        public List<AdditionalCost> AdditionalCosts { get; set; } = new List<AdditionalCost>();

        public int Analysisandrequirementsignoff { get; set; }

        public int FunctionalDesign { get; set; }

        public int TechnicalDesign { get; set; }

        public int AnalysisandDesign1 { get; set; }

        public int Frontendchanges { get; set; }

        public int IntegrationChanges { get; set; }

        public int BackendChanges { get; set; }

        public int Coding { get; set; }

        public int UnitTestCasePreparation { get; set; }

        public int UnittestlogsandDefectFix { get; set; }

        public int CodeReview { get; set; }

        public int UnitTestCaseReview { get; set; }
        public int UnittestResultReview { get; set; }

        public int UnitTesting { get; set; }

        public int QATestCasePreparation { get; set; }

        public int QATestingandDefectFix { get; set; }

        public int IntegrationTesting { get; set; }

        public int UATTestingandDefectFix { get; set; }

        public int QAandTestResultReview { get; set; }

        public int QAandUATSupport { get; set; }

        public int QAandUATTesting { get; set; }


        public int Releasemanagement { get; set; }


        public int DeploymentSupport { get; set; }
        public int WarrantySupport { get; set; }
        public int Support { get; set; }

        public String? AnalysisStartDate { get; set; }
        public String? AnalysisEndDate { get; set; }
        public String? DesignStartDate { get; set; }
        public String? DesignEndDate { get; set; }
        public String? DevStart { get; set; }
        public String? DevEnd { get; set; }
        public String? TestStart { get; set; }
        public String? TestEnd { get; set; }
        public String? UATStart { get; set; }
        public String? UATEnd { get; set; }
        public String? PRODdates { get; set; }
        public String? BCdates { get; set; }

        public string? DeliveryManager { get; set; }

        public string? SeniorManager { get; set; }

        public string? Manager { get; set; }

        public string? ProjectLead { get; set; }

        public string? DevTeamLead { get; set; }

        public string? SeniorDeveloper { get; set; }

        public string? Developer { get; set; }
        public string? QaTeamLead { get; set; }

        public string? SeniorTester { get; set; }

        public string? Tester { get; set; }

        public string? DeploymentTeam { get; set; }


    }

    public class Resource
    {
        public string Name { get; set; } = string.Empty; 
        public float Cost { get; set; }
        public int NumberOfResources { get; set; }
        public float Total => Cost * NumberOfResources;
    }

    public class AdditionalCost
    {
        public string Name { get; set; } = string.Empty;
        public float Cost { get; set; }
        public int NumberOfResources { get; set; }
        public float Total => Cost * NumberOfResources;
    }
}