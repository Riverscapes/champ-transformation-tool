# CHaMP Transformation Tool

The CTT was developed to support the post-processing workflow for topographic and habitat surveys in the **C**olumbia **Ha**biat **M**onitoring **P**rogram ([CHaMP](http://www.champmonitoring.org/)). The tool is an ArcGIS Add-In and when added shows up as a toolbar:

![toolbar]({{site.baseurl}}/assets/images/CTT_Toolbar.png)

* [Download Instructions]({{site/baseurl}}/download.html)
* [How to use CTT]({{site.baseurl}}/how-to-use-ctt.html)
* [Disclaimer & Copyright]({{site.baseurl}}/disclaimer.html)

### What does it do?

The  [Ecogemorphology & Topographic Analysis Lab](http://sites.google.com/a/joewheaton.org/www/lab) (ET-AL) and the [RSGIS Laboratory](http://www.gis.usu.edu/) developed this ArcGIS Add-in for the transformation and projection of [CHaMP](http://www.champmonitoring.org/) field survey data (Bouwes et al. 2011) into real world coordinates given hand-held GPS coordinates from just three benchmarks. The CTT will work with any unprojected data in a file geodatabase, and gives you multiple ways to do an afine transformation based on a simple shift and rotation. Although other transformation tools exist in ArcGIS, there is not a simple sequence of geoprocessing tools, which allow you to transform vector data in a manner that preserves the high relative accuracy of a total station survey. See Wheaton et al. (2012) and the [ReadMe]() page for more background.

![abstract]({{site.baseurl}}/assets/images/CTT_GraphicalAbstract.jpg)

## News! CTT Receives Award for Best Desktop GIS Software

![ESRI]({{site.baseurl}}/assets/images/esri_award.jpg)

Chris Garrard, who developed the CHaMP Transformation Tool for [ET-AL](http://etal.joewheaton.org/) recently presented the tool at the [2012 ESRI International User Conference](http://www.esri.com/events/user-conference/). She was awarded third place for best  Desktop GIS Software Application in the [User Software Application Fair](http://www.esri.com/events/user-conference/participate/user-app-fair-results.html).

#### Funding

Funding for the CTT was provided by the Bonneville Power Administration (BPA Proposal # 2011-006-00 & BPA Project: 2003-017-00 ) and NOAA as part of the [Columbia Habitat Monitoring Protocol](http://www.champmonitoring.org/) (CHaMP).

![BPA]({{site.baseurl}}/assets/images/BPA.png)

## Tool Development

The CHaMP Transformation Tool was developed by [Joe Wheaton]([http://www.joewheaton.org/lab](http://www.joewheaton.org/lab)) and [Chris Garrard](http://www.gis.usu.edu/~chrisg)) at [Utah State University]([http://cnr.usu.edu/wats](http://cnr.usu.edu/wats)), with design input from Carol Volk and Kelly Whitehead ([South Fork Research](http://southforkresearch.org)).  For more information, see  [http://www.champmonitoring.org](http://www.champmonitoring.org/).

![etal]({{site.baseurl}}/assets/images/etal_lab.png)

#### References

- Bouwes N, Moberg J, Weber N, Bouwes B, Bennett S, Beasley C, Jordan CE, Nelle P, Polino S, Rentmeester S, Semmens B, Volk C, Ward MB and White J. 2011. [Scientific Protocol for Salmonid Habitat Survyes within the Columbia Habitat Monitoring Program](http://www.pnamp.org/sites/default/files/CHaMPHabitatProtocol_20110125_0.pdf), Prepared by the Integrated Status and Effectiveness Monitoring Program and published by Terraqua, Inc., Wauconda, WA, 118 pp. 
- Wheaton JM, Garrard C, Volk C, Whitehead K and Bouwes N. 2012. [A Simple, Interactive GIS Tool for Transforming Assumed Total Station Surveys to Real World Coordinates - The CHaMP Transformation Tool](https://www.researchgate.net/publication/242653749_A_Simple_Interactive_GIS_Tool_for_Transforming_Assumed_Total_Station_Surveys_to_Real_World_Coordinates_-_The_CHaMP_Transformation_Tool). Submitted to Computers & Geosciences.42: 28-36. DOI: [10.1016/j.cageo.2012.02.003](http://dx.doi.org/10.1016/j.cageo.2012.02.003).