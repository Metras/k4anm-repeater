/*
Copyright © 2007 Nik Martin, K4ANM <><

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections;
using System.Configuration.Install;
using System.ServiceProcess;
using System.ComponentModel;

[RunInstallerAttribute(true)]
public class ProjectInstaller :Installer
	{

	private ServiceInstaller serviceInstaller;
	private ServiceProcessInstaller processInstaller;

	public ProjectInstaller()
		{

		processInstaller = new ServiceProcessInstaller();
		serviceInstaller = new ServiceInstaller();

		// Service will run under system account
		processInstaller.Account = ServiceAccount.LocalSystem;

		// Service will have Start Type of Manual
		serviceInstaller.StartType = ServiceStartMode.Manual;

		serviceInstaller.ServiceName = "K4ANM Repeater Controller";
		serviceInstaller.Description = "An Amateur Radio Repeater Controller System";

		Installers.Add(serviceInstaller);
		Installers.Add(processInstaller);
		}
	}