This folder contains a custom build of the December 2011 Silverlight toolkit.
It is built from the source code at:

SVN URL: https://syd1tfs02.entdata.local/svn/EntData/ThirdParty/Microsoft/Silverlight/v5.0/Toolkit/Dec2011

Run GetFilesFromBuildOutput.cmd to copy the files from the custom build location into this branch.
This command should only be executed when changes are made to the custom toolkit and the files need to be sycned when the current branch.

Run PublishFilesToSDKLocation.cmd to publish the files to the Silverlight toolkit install location.
This is necessary so that the Visual Studio adds the correct assembly references when controls are added via the designer.