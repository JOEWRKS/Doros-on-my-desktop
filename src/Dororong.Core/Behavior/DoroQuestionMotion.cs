namespace Dororong.Core.Behavior;

public static class DoroQuestionMotion
{
    private static double Smooth(double value){var t=Math.Clamp(value,0,1);return t*t*(3-2*t);}
    // The authored face points left; negative roll tilts toward its front.
    // The presenter's existing mirror also reverses the tilt when facing right.
    public static double HeadRoll(double phase)=>-8*Math.PI/180*Smooth(phase/.18)*(1-Smooth((phase-.76)/.24));
    public static double BubbleOpacity(double phase)=>Smooth((phase-.08)/.10)*(1-Smooth((phase-.85)/.15));
}
