# Third-Party Notices

CoreVigil itself is intended to be distributed under the GNU General Public License version 3 (GPL-3.0).

The CoreVigil application project does not currently declare third-party NuGet package dependencies. The test project (`CoreVigil.Tests`) uses the packages listed below. These packages retain their own licenses and copyright notices.

## Direct test dependencies

| Package | Version | Declared license |
|---|---:|---|
| coverlet.collector | 6.0.4 | MIT |
| Microsoft.NET.Test.Sdk | 17.14.1 | MIT |
| xunit | 2.9.3 | Apache-2.0 |
| xunit.runner.visualstudio | 3.1.4 | Apache-2.0 |

## Transitive test dependencies identified during the public-release audit

| Package | Version | Declared license |
|---|---:|---|
| Microsoft.CodeCoverage | 17.14.1 | MIT |
| Microsoft.TestPlatform.ObjectModel | 17.14.1 | MIT |
| Microsoft.TestPlatform.TestHost | 17.14.1 | MIT |
| Newtonsoft.Json | 13.0.3 | MIT |
| xunit.abstractions | 2.0.3 | License reference supplied by the package metadata; upstream xUnit license information identifies Apache-2.0 as the primary project license and also records imported MIT-licensed components |
| xunit.analyzers | 1.18.0 | Apache-2.0 |
| xunit.assert | 2.9.3 | Apache-2.0 |
| xunit.core | 2.9.3 | Apache-2.0 |
| xunit.extensibility.core | 2.9.3 | Apache-2.0 |
| xunit.extensibility.execution | 2.9.3 | Apache-2.0 |

## Notes

- This file documents third-party packages identified from `CoreVigil.Tests.csproj` and its restored transitive dependency graph during the public-release audit.
- Third-party software is not relicensed by CoreVigil. Each dependency remains subject to its upstream license and notices.
- Package versions and dependency relationships can change in future revisions. Contributors should review this file whenever test dependencies are added, removed, or upgraded.
- The CoreVigil application and tray icon artwork was generated specifically for the CoreVigil project and is not identified as third-party artwork in the current repository audit.

## Upstream license references

- GNU GPLv3: https://www.gnu.org/licenses/gpl-3.0.html
- Apache License 2.0: https://www.apache.org/licenses/LICENSE-2.0
- MIT License: https://opensource.org/license/mit
- xUnit.net: https://xunit.net/
- NuGet: https://www.nuget.org/

This notice is informational and is not a substitute for the license text distributed by each dependency's upstream project or package.
