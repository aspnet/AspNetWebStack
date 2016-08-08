// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.Helpers
{
    public static class ChartTheme
    {
        // Review: Need better names.

        public const string Blue =
            @"<Chart BackColor=""#D3DFF0"" BackGradientStyle=""TopBottom"" BackSecondaryColor=""White"" BorderColor=""26, 59, 105"" BorderlineDashStyle=""Solid"" BorderWidth=""2"" Palette=""BrightPastel"">
    <ChartAreas>
        <ChartArea Name=""Default"" _Template_=""All"" BackColor=""64, 165, 191, 228"" BackGradientStyle=""TopBottom"" BackSecondaryColor=""White"" BorderColor=""64, 64, 64, 64"" BorderDashStyle=""Solid"" ShadowColor=""Transparent"" /> 
    </ChartAreas>
    <Legends>
        <Legend _Template_=""All"" BackColor=""Transparent"" Font=""Trebuchet MS, 8.25pt, style=Bold"" IsTextAutoFit=""False"" /> 
    </Legends>
    <BorderSkin SkinStyle=""Emboss"" /> 
  </Chart>";

        public const string Green =
            @"<Chart BackColor=""#C9DC87"" BackGradientStyle=""TopBottom"" BorderColor=""181, 64, 1"" BorderWidth=""2"" BorderlineDashStyle=""Solid"" Palette=""BrightPastel"">
  <ChartAreas>
    <ChartArea Name=""Default"" _Template_=""All"" BackColor=""Transparent"" BackSecondaryColor=""White"" BorderColor=""64, 64, 64, 64"" BorderDashStyle=""Solid"" ShadowColor=""Transparent"">
      <AxisY LineColor=""64, 64, 64, 64"">
        <MajorGrid Interval=""Auto"" LineColor=""64, 64, 64, 64"" />
        <LabelStyle Font=""Trebuchet MS, 8.25pt, style=Bold"" />
      </AxisY>
      <AxisX LineColor=""64, 64, 64, 64"">
        <MajorGrid LineColor=""64, 64, 64, 64"" />
        <LabelStyle Font=""Trebuchet MS, 8.25pt, style=Bold"" />
      </AxisX>
      <Area3DStyle Inclination=""15"" IsClustered=""False"" IsRightAngleAxes=""False"" Perspective=""10"" Rotation=""10"" WallWidth=""0"" />
    </ChartArea>
  </ChartAreas>
  <Legends>
    <Legend _Template_=""All"" Alignment=""Center"" BackColor=""Transparent"" Docking=""Bottom"" Font=""Trebuchet MS, 8.25pt, style=Bold"" IsTextAutoFit =""False"" LegendStyle=""Row"">
    </Legend>
  </Legends>
  <BorderSkin SkinStyle=""Emboss"" />
</Chart>";

        public const string Vanilla =
            @"<Chart Palette=""SemiTransparent"" BorderColor=""#000"" BorderWidth=""2"" BorderlineDashStyle=""Solid"">
<ChartAreas>
    <ChartArea _Template_=""All"" Name=""Default"">
            <AxisX>
                <MinorGrid Enabled=""False"" />
                <MajorGrid Enabled=""False"" />
            </AxisX>
            <AxisY>
                <MajorGrid Enabled=""False"" />
                <MinorGrid Enabled=""False"" />
            </AxisY>
    </ChartArea>
</ChartAreas>
</Chart>";

        public const string Vanilla3D =
            @"<Chart BackColor=""#555"" BackGradientStyle=""TopBottom"" BorderColor=""181, 64, 1"" BorderWidth=""2"" BorderlineDashStyle=""Solid"" Palette=""SemiTransparent"" AntiAliasing=""All"">
    <ChartAreas>
        <ChartArea Name=""Default"" _Template_=""All"" BackColor=""Transparent"" BackSecondaryColor=""White"" BorderColor=""64, 64, 64, 64"" BorderDashStyle=""Solid"" ShadowColor=""Transparent"">
            <Area3DStyle LightStyle=""Simplistic"" Enable3D=""True"" Inclination=""30"" IsClustered=""False"" IsRightAngleAxes=""False"" Perspective=""10"" Rotation=""-30"" WallWidth=""0"" />
        </ChartArea>
    </ChartAreas>
</Chart>";

        public const string Yellow =
            @"<Chart BackColor=""#FADA5E"" BackGradientStyle=""TopBottom"" BorderColor=""#B8860B"" BorderWidth=""2"" BorderlineDashStyle=""Solid"" Palette=""EarthTones"">
  <ChartAreas>
    <ChartArea Name=""Default"" _Template_=""All"" BackColor=""Transparent"" BackSecondaryColor=""White"" BorderColor=""64, 64, 64, 64"" BorderDashStyle=""Solid"" ShadowColor=""Transparent"">
      <AxisY>
        <LabelStyle Font=""Trebuchet MS, 8.25pt, style=Bold"" />
      </AxisY>
      <AxisX LineColor=""64, 64, 64, 64"">
        <LabelStyle Font=""Trebuchet MS, 8.25pt, style=Bold"" />
      </AxisX>
    </ChartArea>
  </ChartAreas>
  <Legends>
    <Legend _Template_=""All"" BackColor=""Transparent"" Docking=""Bottom"" Font=""Trebuchet MS, 8.25pt, style=Bold"" LegendStyle=""Row"">
    </Legend>
  </Legends>
  <BorderSkin SkinStyle=""Emboss"" />
</Chart>";
    }
}
