using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoQuoteLibrary.AutoQuoteHelper
{
    public class CacheExpiration
    {
        private String _minute = "*";
        private String _hour = "*";
        private String _dayOfMonth = "*";
        private String _month = "*";
        private String _dayOfWeek = "*";

        public CacheExpiration()
        {

        }

        public String Minute
        {
            get
            {
                return _minute;
            }
            set
            {
                _minute = value;
            }
        }

        public String Hour
        {
            get
            {
                return _hour;
            }
            set
            {
                _hour = value;
            }
        }

        public String DayOfMonth
        {
            get
            {
                return _dayOfMonth;
            }
            set
            {
                _dayOfMonth = value;
            }
        }

        public String Month
        {
            get
            {
                return _month;
            }
            set
            {
                _month = value;
            }
        }

        public String DayOfWeek
        {
            get
            {
                return _dayOfWeek;
            }
            set
            {
                _dayOfWeek = value;
            }
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Minute.Trim());
            sb.Append(" ");
            sb.Append(Hour.Trim());
            sb.Append(" ");
            sb.Append(DayOfMonth.Trim());
            sb.Append(" ");
            sb.Append(Month.Trim());
            sb.Append(" ");
            sb.Append(DayOfWeek.Trim());
            return sb.ToString();
        }
    }
}
