using MediaPlayerApp.ViewModels;

namespace MediaPlayerApp.Tests;

public class UrlInputDialogViewModelTests
{
    [Fact]
    public void ConfirmCommand_ClosesDialogWithTrue_WhenValidHttpUrl()
    {
        var vm = new UrlInputDialogViewModel("https://example.com/video.mp4");
        bool? closedWithResult = null;
        vm.RequestClose += (sender, result) => closedWithResult = result;

        vm.ConfirmCommand.Execute(null);

        Assert.True(closedWithResult);
        Assert.Null(vm.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-valid-url")]
    [InlineData("ftp://example.com/media.mp4")]
    public void ConfirmCommand_SetsErrorMessage_WhenInvalidUrl(string invalidUrl)
    {
        var vm = new UrlInputDialogViewModel(invalidUrl);
        bool? closedWithResult = null;
        vm.RequestClose += (sender, result) => closedWithResult = result;

        vm.ConfirmCommand.Execute(null);

        Assert.Null(closedWithResult);
        Assert.NotNull(vm.ErrorMessage);
    }

    [Fact]
    public void SelectPresetCommand_UpdatesUrlText()
    {
        var vm = new UrlInputDialogViewModel();
        UrlPreset preset = vm.Presets[1];

        vm.SelectPresetCommand.Execute(preset);

        Assert.Equal(preset.Url, vm.UrlText);
    }
}
