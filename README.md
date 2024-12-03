<!-- PROJECT LOGO -->
<div align="center">
  <img src="images/PET.svg" alt="Logo" width="256" height="256">
  <h3 align="center">The PET Group</h3>
  <p align="center">
    DeskMotion for desk stuff.
    <br />
    <a href="https://github.com/SP-SDU/DeskMotion"><strong>View Project »</strong></a>
    <br />
    <br />
    <a href="https://github.com/SP-SDU/DeskMotion/issues">Issues</a>
    ·
    <a href="https://github.com/orgs/SP-SDU/projects/5/views/1">Board</a>
  </p>
</div>

## Getting Started 🚀

**Installation:**
1. Download the latest release from [releases](https://github.com/SP-SDU/DeskMotion/releases/).
2. Follow the installation guide provided with the release.

## Running with Docker 🐳

1. **Set up your `.env` file:**
   - Copy the `example.env` file to `.env` in the project root:
     ```bash
     cp example.env .env
     ```

2. **Run the application:**
   - **Note**: Set up HTTPS certificates first (see below).
     ```bash
     docker-compose up
     ```

## Setting Up Dev HTTPS Certificates 🛡️

1. Open the terminal in the project root and trust the HTTPS certificate:
   - **Note**: You **MUST** close all the entire browser for it to take affect.
   ```bash
   dotnet dev-certs https --trust -ep certs/LocalhostDevCert.pfx -p postgres
   ```

Will be replaced by a real Cert from Let's Encrypt in Production.


## Updating Migrations in the Asp.NET Project 🖱️

If you make changes to the application database context, follow these steps to update the migrations:

1. **Delete the ****`Migrations`**** folder** in the project directory.
2. **Run the following command** in the Razor Pages project directory:
   ```bash
   dotnet ef migrations add Init
   ```

This will create a new initial migration reflecting the changes to the database context.

## Contributing 🤝

1. **Clone** Open [GitHub Desktop](https://desktop.github.com/), go to `File > Clone Repository`, and enter:
     ```
     https://github.com/SP-SDU/DeskMotion
     ```
2. **Branch**: In GitHub Desktop, switch to `main` and create a new branch (e.g., `add-login-feature`).
3. **Commit & Push**: Commit changes in GitHub Desktop, then click `Push origin`.
4. **Pull Request**: Open a pull request on GitHub, choosing `main` as the base branch, and tag a teammate for review.

For more details, see [GitHub Flow](https://githubflow.github.io/).

## Communication 🗂️

Join the [Discord server](https://discord.gg/b6sdqaTbsU) for discussions and updates.

## License 📝

Distributed under the Apache 2.0 License. See [LICENSE](LICENSE) for details.

