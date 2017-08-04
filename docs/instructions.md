# Read Me

***
[![toolbar]({{site.baseurl}}/assets/images/CTT_Toolbar.png)]({{site.baseurl}}/index.html)
CHaMP Transformation Tool 1.0 Beta - ArcGIS Plugin

[![img]({{site.baseurl}}/assets/images/CTT_About.png)]({{site.baseurl}}/assets/images/CTT_About.png)




Produced by Chris Garrard & Joe Wheaton 

Copyright © 2011 Wheaton

Updated:  May, 2011



## GNU Licnese

Developer can be contacted through the [GitHub repo](https://github.com/Riverscapes/champ-transformation-tool ). The source code is licensed under a [GNU Public License](https://github.com/Riverscapes/champ-transformation-tool/blob/master/LICENSE) as open source and can be downloaded by developers wishing to extend the tool [here](https://github.com/Riverscapes/champ-transformation-tool ) (requires Visual Studio, C#).

This [program]({{site.baseurl}}/download) is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version.  

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program; if not, write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA or see [http://www.gnu.org/licenses/gpl-2.0.txt](http://www.gnu.org/licenses/gpl-2.0.txt) (GLP v2 license).

## What does it do?

The [Ecogemorphology & Topographic Analysis Lab](https://sites.google.com/a/joewheaton.org/www/lab) (ET-AL) and the RSGIS Center developed this ArcGIS Add-in for the transformation and projection of CHAMP field survey data into UTM coordinate system given GPS coordinates from three benchmarks.  The tool will work with any unprojected data in a file geodatabase, and gives you multiple ways to do a simple transformation based on a shift and rotate. Although other transformation tools exist in ArcGIS, there is not a simple sequence of geoprocessing tools, which allow you to transform vector data in a manner that preserves the high relative accuracy of a total station survey.

## Background

The CHaMP Transformation Tool is designed to work very efficiently with data collected in accordance with the[CHaMP survey protocol](http://www.champmonitoring.org/), but will also work for other assumed surveys. This tool is used by field crews in post-processing their topographic and habitat survey data to give them a rough projected coordinate system for their data. This tool would only be used on the first visit to the site. The tool’s primary functions are to**:**

●     Establish a Quasi-UTM coordinate system transformation from the original assumed total station Cartesian coordinate survey into a UTM projection that is rough, but good enough to support rough overlay with other GIS layers (e.g.  contextual aerial imagery and 10 m USGS DEMs)

●     Import raw survey data from total station (points, polyline and polygon files) and transform data into this new Quasi-UTM coordinate system and save outputs in file geodatabase

●     Assist the user in choosing the ‘best’ transformation input parameters (via a shift, rotate and scale transformation; i.e. an affine transformation) based on all possible combinations of survey benchmarks/bearings, through allowing user to review and choose the best fit out of 9 possible transformations by showing table of residual errors (based on using third unused benchmark as a check) & showing a preview of the overlay in the map data display of ArcGIS.

●     Record Transformation Model, and updated Control Points for site reoccupation. Write a summary table report.

------

## Requisites

### User Requirements:

- Some understanding of GIS, and the difference between unprojected data (i.e. an assumed cartesian coordinate system) and projected coordinate systems (e.g. UTM)
- Familiarity with ArcGIS

### System Requirements:

- ArcGIS 10.X 

### Input Data Requirements:

You need unprojected survey data, which was surveyed in an assumed Cartesian coordinate system, and three benchmarks spread out over the area of interest. The basic requirements are:

- The input unprojected survey data are feature classes in a file geodatabase (point, polyline and polygon) will work
- The three benchmarks you surveyed were surveyed both in the unprojected coordinate system (i.e. with a total station) and were located in real geographic space (e.g. with a hand-held GPS or even crudely identified on a projected aerial photograph)

------

## Installation Procedure 

### Installing

To install, make sure that ArcGIS  is not running and any previous versions of the Add-In are removed. Simply double click on the `[`CHAMP.esriAddIn`](http://etalweb.joewheaton.org/CHAMP_TransformationTool/10_1/CHAMP.esriAddIn)` after you download [it](http://etalweb.joewheaton.org/CHAMP_TransformationTool/10_1/CHAMP.esriAddIn). See this video for step-by-step instructions:

<iframe width="560" height="315" src="https://www.youtube.com/embed/Gz3jtxLN474" frameborder="0" allowfullscreen></iframe>

### Uninstalling

If you ever need to uninstall the add-in, simply navigate to Customize -> Add-Ins, and remove it. See also:

<iframe width="560" height="315" src="https://www.youtube.com/embed/_WtbJKicI8I" frameborder="0" allowfullscreen></iframe>

### 

### 

## Support

No formal support is available with this free software. However, do feel free to contact us if you find bugs or have questions. CHaMP crews should use the [CHaMP help forum](http://forum.bluezone.usu.edu/champ/viewforum.php?f=5).

## 

## Revision History

The CTT 1.0.0 was modified on 3/4/2012 to address the following (see Bitbucket repo for original revision history and new GitHub repo for current changes):

Minor Bugs:

- Saving Layers wrong
- Transformation Preview
- An occasional error/warning message
- Tab order
- Sometimes coordinate system combobox was not filled
- Made the about dialog a little bigger

Added the following features:

- Shapefile support (previously only worked with file geodatabases)
- Projection Support (now works for any user specified projection)
- Benchmark bearings can now be added fro a file
- She also added the GNU open source license to the code

## Acknowledgements

The CHaMP Transformation Tool was developed by [Joe Wheaton] (http://www.joewheaton.org/lab) and [Chris Garrard] (http://www.gis.usu.edu/~chrisg) at [Utah State University ](http://cnr.usu.edu/wats), with design input from Carol Volk and Kelly Whitehead ([South Fork Research, Inc.](http://southforkresearch.org/)). Funding for this tool was provided by the Bonneville Power Administration (BPA Proposal # 2011-006-00) and NOAA as part of the CHaMP ([Columbia Habitat Monitoring Protocol](http://www.champmonitoring.org)). 

.