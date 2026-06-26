public class ToastCmd
{
    private ViewModel viewModel => ViewModel.Instance;

    private string msg;
    public ToastCmd(string msg)
    {
        this.msg = msg;
    }
    public void Run()
    {
        viewModel.ShowToast(new ToastMsg { text = msg });
        new SoundCmd(SoundModel.Instance.ERROR).Run();
    }

    public void Run(BuildingName buildingName)
    {
        viewModel.ShowToast(new ToastMsg { text = msg, challenge = buildingName });
        new SoundCmd(SoundModel.Instance.MAGIC_LIGHT).Run();
    }
}