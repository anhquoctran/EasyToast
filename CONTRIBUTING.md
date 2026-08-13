# Contributing to FuzzyToast

Thanks for helping improve the library. FuzzyToast targets **Windows 10/11** WinForms on **.NET Framework 4.6.1+** and **.NET 8+**.

## Development setup

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) on Windows.
2. Clone the repo and restore:

```bash
git clone https://github.com/anhquoctran/FuzzyToast.git
cd FuzzyToast
dotnet restore EasyToast.slnx
```

## Build, test, coverage

```bash
dotnet build EasyToast.slnx -c Release
dotnet test EasyToast.slnx -c Release
```

Line coverage must stay **≥ 95%** (designer/generated files excluded):

```powershell
./scripts/test-coverage.ps1
```

WinForms / STA tests run on a dedicated STA thread (`StaHelper`). Run the suite on Windows.

## Pack locally

```powershell
./scripts/pack.ps1
```

Output: `artifacts/nuget/`.

## Project layout

| Path | Role |
|------|------|
| `src/FuzzyToast/` | Library (public API, layout, WinForms view) |
| `tests/FuzzyToast.Tests/` | xUnit + Coverlet |
| `samples/EasyToastDemo/` | Manual demo app |
| `docs/` | GitHub Pages site (getting started, migration, design) |

Public types live in the `FuzzyToast` namespace. Keep new UI-thread work behind `IUiMarshaler` so unit tests can inject fakes.

## Pull requests

- Open an issue first for larger API changes.
- Keep PRs focused; match existing naming and tab indentation in `.cs` files.
- Update `CHANGELOG.md` for user-visible changes.
- Fill in the PR template checklist.

## Releasing (maintainers)

1. Bump `<Version>` in `src/FuzzyToast/FuzzyToast.csproj` and add a `CHANGELOG.md` section.
2. Commit, tag, and push:

```bash
git tag v3.0.2
git push origin v3.0.2
```

3. The **Release** workflow packs, creates a GitHub Release, and publishes to NuGet.org when the `NUGET_API_KEY` repository secret is set.
