namespace SHIV_Data_Weigh
{
	public class SylvacObj : ObservableObject
	{
		private float floatValue;
		public float FloatValue
        {
			get { return floatValue; }
			set
			{
                floatValue = value;
				OnPropertyChanged("FloatValue");
			}
		}

		public SylvacObj()
		{
            floatValue = 0;
		}
	}
}
