namespace ICan.Jobs.WB
{
	public class AlertResult
	{
		public bool Mono { get; set; }

		public bool MonoPallette { get; set; }

		public bool NeedAlert => Mono || MonoPallette;
	}
}
