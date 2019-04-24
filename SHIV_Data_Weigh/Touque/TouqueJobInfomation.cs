using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHIV_Data_Weigh
{
    public class TouqueJobInfomation
    {
        // Tổng số lượng Job
        public int numberOfJobPart { get; set; }
        // Giá trị công việc cuối cùng của Job cuối cùng - làm thông tin hoàn thành Job
        public int finishNumber { get; set; }
        public TouqueJobInfomation(int JobNum, int FinishNum)
        {
            numberOfJobPart = JobNum;
            finishNumber = FinishNum;
        }
    }
}
