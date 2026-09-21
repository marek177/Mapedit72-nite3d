namespace MapEdit72N3D.UI;
public sealed class FindReplaceForm : Form
{
    public NumericUpDown FindValue {get;}=new(){Minimum=0,Maximum=255,Width=80};
    public NumericUpDown ReplaceValue {get;}=new(){Minimum=0,Maximum=255,Width=80};
    public ComboBox LayerBox {get;}=new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=110};
    public Button FindAllButton {get;}=new(){Text="Find / Highlight",Width=120};
    public Button ReplaceAllButton {get;}=new(){Text="Replace all",Width=100};
    public FindReplaceForm()
    {
        Text="Find / Replace"; FormBorderStyle=FormBorderStyle.FixedToolWindow; StartPosition=FormStartPosition.CenterParent; Width=390; Height=150;
        LayerBox.Items.AddRange(new object[]{"Walls","Objects"}); LayerBox.SelectedIndex=0;
        var p=new FlowLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(10)};
        p.Controls.AddRange(new Control[]{new Label{Text="Layer",AutoSize=true,Margin=new Padding(0,7,4,0)},LayerBox,new Label{Text="Find",AutoSize=true,Margin=new Padding(8,7,4,0)},FindValue,new Label{Text="Replace",AutoSize=true,Margin=new Padding(8,7,4,0)},ReplaceValue,FindAllButton,ReplaceAllButton}); Controls.Add(p);
    }
}
