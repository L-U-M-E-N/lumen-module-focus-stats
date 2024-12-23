namespace Lumen.Module.FocusStats.Business.Helpers {
    public static class ActivityCompressionHelper {
        public static void CompressData(this IList<UserFocusedActivity> activities) {
            for (int i = 0; i < activities.Count - 1; i++) {
                var newStart = activities[i].StartTime.AddSeconds(activities[i].SecondsDuration);

                if (
                    activities[i].ProgramName == activities[i + 1].ProgramName &&
                    activities[i].ProgramExe == activities[i + 1].ProgramExe &&
                    newStart.CompareTo(activities[i].StartTime) == 0
                    ) {

                    activities[i].SecondsDuration += activities[i + 1].SecondsDuration;
                    activities.RemoveAt(i + 1);

                    i--;
                }
            }
        }
    }
}
