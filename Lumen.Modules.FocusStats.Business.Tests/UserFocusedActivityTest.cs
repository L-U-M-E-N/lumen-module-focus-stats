using Lumen.Modules.FocusStats.Common;
using Lumen.Modules.FocusStats.Common.Rules;

namespace Lumen.Modules.FocusStats.Business.Tests {
    public class UserFocusedActivityTest {
        [Fact]
        public void UserFocusedActivity_Standard_0_Rules() {
            var seconds = 42;
            var exe = "exe";
            var name = "name";
            var startTime = DateTime.UtcNow;

            var activity = UserFocusedActivity.CreateActivityAsync(
                [],
                startTime,
                seconds,
                exe,
                name
            );

            Assert.NotNull(activity);
            Assert.Equal(seconds, activity.SecondsDuration);
            Assert.Equal(name, activity.ProgramName);
            Assert.Equal(exe, activity.ProgramExe);
            Assert.Equal(startTime, activity.StartTime);
        }

        [Fact]
        public void UserFocusedActivity_EmptyExe() {
            Assert.Throws<ArgumentNullException>(() => {
                UserFocusedActivity.CreateActivityAsync(
                    [],
                    DateTime.UtcNow,
                    42,
                    "",
                    "name"
                );
            });
        }

        [Fact]
        public void UserFocusedActivity_EmptyName() {
            var activity = UserFocusedActivity.CreateActivityAsync(
                 [],
                 DateTime.UtcNow,
                 42,
                 "exe",
                 ""
             );

            Assert.Equal("?", activity.ProgramName);
        }

        private static readonly IEnumerable<CleaningRule> Rules = [
            new CleaningRule("^.*(Discord).*$", "Discord", RuleTarget.Both, []),
            new CleaningRule("[0-9]$", "", RuleTarget.Both, []),
            new CleaningRule("42a$", "", RuleTarget.Exe, []),
            new CleaningRule("ABC$", "DEF", RuleTarget.Name, []),
        ];

        [InlineData("exe", "name", "exe", "name")]
        [InlineData("test/Discord/test", "name", "Discord", "name")]
        [InlineData("exe", "test/Discord/test", "exe", "Discord")]
        [InlineData("sdfhoihiosdfohijdsfDiscordsdgjdsgjgdsj", "test/Discord/test", "Discord", "Discord")]
        [InlineData("xyz1", "name", "xyz", "name")]
        [InlineData("xyz42a", "name", "xyz", "name")]
        [InlineData("xyz42a", "xyz42a", "xyz", "xyz42a")]
        [InlineData("xyzABC", "xyzABC", "xyzABC", "xyzDEF")]
        [Theory]
        public void UserFocusedActivity_Standard_4_Rules(string inputExe, string inputName, string expectedExe, string expectedName) {
            var seconds = 42;
            var startTime = DateTime.UtcNow;

            var activity = UserFocusedActivity.CreateActivityAsync(
                Rules,
                startTime,
                seconds,
                inputExe,
                inputName
            );

            Assert.NotNull(activity);
            Assert.Equal(seconds, activity.SecondsDuration);
            Assert.Equal(expectedName, activity.ProgramName);
            Assert.Equal(expectedExe, activity.ProgramExe);
            Assert.Equal(startTime, activity.StartTime);
        }
    }
}