<!-- PROJECT LOGO -->
<div align="center">
  <img src="images/PET.avif" alt="Logo" width="256" height="256">
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

1. Create a `.env` file in the project root with:

   - **Windows**:
     ```dotenv
     SECRETS_PATH=${APPDATA}/Microsoft
     HTTPS_PATH=${APPDATA}/ASP.NET
     ```

   - **Linux/macOS**:
     ```dotenv
     SECRETS_PATH=${HOME}/.microsoft
     HTTPS_PATH=${HOME}/.aspnet
     ```

2. Run:

   ```bash
   docker-compose up
   ```

This sets paths for user secrets and HTTPS certificates which ASP.NET requires.

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

