using Lumen.Module.FocusStats.Business.Rules;

namespace Lumen.Module.FocusStats.Business.Tests {
    public class UserFocusedActivityTest {
        [Fact]
        public void UserFocusedActivity_Standard_0_Rules() {
            var activity = UserFocusedActivity.CreateActivityAsync(
                [],
                DateTime.UtcNow,
                42,
                "exe",
                "name"
            );

            activity.Should().NotBeNull();
            activity.SecondsDuration.Should().Be(42);
            activity.ProgramName.Should().Be("name");
            activity.ProgramExe.Should().Be("exe");
            activity.StartTime.Should().BeBefore(DateTime.UtcNow);
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
            Assert.Throws<ArgumentNullException>(() => {
                UserFocusedActivity.CreateActivityAsync(
                    [],
                    DateTime.UtcNow,
                    42,
                    "exe",
                    ""
                );
            });
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
            var activity = UserFocusedActivity.CreateActivityAsync(
                Rules,
                DateTime.UtcNow,
                42,
                inputExe,
                inputName
            );

            activity.Should().NotBeNull();
            activity.SecondsDuration.Should().Be(42);
            activity.ProgramName.Should().Be(expectedName);
            activity.ProgramExe.Should().Be(expectedExe);
            activity.StartTime.Should().BeBefore(DateTime.UtcNow);
        }
    }
}