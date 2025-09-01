using System;

namespace RM.Database.KingResearchContext
{
    public class ScreenerTables
    {
        public class ScreenerCategoryM
        {
            public int Id { get; set; } // Primary Key
            public string CategoryName { get; set; } // Name of the category
            public string Description { get; set; } // Optional description
            public bool IsActive { get; set; } // Indicates if the category is active
            public DateTime CreatedDate { get; set; } // Record creation timestamp
            public DateTime? ModifiedDate { get; set; } // Record modification timestamp
        }

        public class ScreenerM
        {
            public int Id { get; set; } // Primary Key
            public int CategoryId { get; set; } // Foreign Key to ScreenerCategory
            public string Name { get; set; } // Name of the screener
            public string Code { get; set; } // Unique code for the screener
            public string Description { get; set; } // Optional description of the screener
            public bool IsActive { get; set; } // Indicates if the screener is active
            public DateTime CreatedDate { get; set; } // Record creation timestamp
            public DateTime? ModifiedDate { get; set; } // Record modification timestamp

            // Navigation Property
            public ScreenerCategoryM Category { get; set; } // Reference to the related ScreenerCategory
        }

        /// <summary>
        /// Use to store teh data for 
        /// </summary>
        public class ScreenerStockDataM
        {
            public long Id { get; set; }
            public int ScreenerId { get; set; }
            public int SymbolId { get; set; }
            public decimal TriggerPrice { get; set; }
            public DateTime ModifiedOn { get; set; }
        }
    }
}
