using System.Text;
using ImGuiNET;

namespace XivMate.DataGathering.Forays.Dalamud.Extensions;

public class ImGuiHelper
{
    public static bool InputText(string label, ref string text, uint bufferSize = 256)
    {
        var buffer = Encoding.UTF8.GetBytes(text);
        if (buffer.Length > bufferSize - 1)
        {
            buffer = Encoding.UTF8.GetBytes(text.Substring(0, (int)bufferSize - 1));
        }

        var inputTextBuffer = new byte[bufferSize];
        System.Buffer.BlockCopy(buffer, 0, inputTextBuffer, 0, buffer.Length);
        //ImGui.Text(label);
        ImGui.PushItemWidth(ImGui.CalcTextSize($"{label}").X + ImGui.GetStyle().ItemInnerSpacing.X);
        ImGui.Text($"{label}");
        ImGui.PopItemWidth();
        ImGui.SameLine();

        if (ImGui.InputText($"##{label}", inputTextBuffer, bufferSize))
        {
            text = Encoding.UTF8.GetString(inputTextBuffer).TrimEnd('\0'); //remove trailing null chars.
            return true;
        }

        return false;
    }
}
