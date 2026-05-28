# Control Deployments using Release Gates with YAML Pipelines

**Estimated time:** 75 minutes

You will learn how to configure deployment gates using YAML multi-stage pipelines and Azure DevOps **Environments** with **Approvals and Checks**. You'll build a single pipeline that compiles the application and deploys to a DevTest environment only after manual approval, and gates promotion to Production by querying Azure Monitor for active alerts against the DevTest web app.

This lab takes approximately 75 minutes to complete.

---

## Before you start

You need:

- **Microsoft Edge** or an Azure DevOps-supported browser
- **Azure subscription:** An active Azure subscription, or create a new one
- **Azure DevOps organization:** Create one at https://learn.microsoft.com/azure/devops/organizations/accounts/create-organization if you don't have one
- **Account permissions:** A Microsoft account or Microsoft Entra account with:
  - **Owner** role in the Azure subscription
  - **Global Administrator** role in the Microsoft Entra tenant
  - For details, see [List Azure role assignments using the Azure portal](https://learn.microsoft.com/azure/role-based-access-control/role-assignments-list-portal) and [View and assign administrator roles in Azure Active Directory](https://learn.microsoft.com/azure/active-directory/roles/manage-roles-portal)

---

## About Approvals and Checks in YAML Pipelines

In classic release pipelines, gates and approvals are configured *inside* the pipeline definition on each stage. In YAML multi-stage pipelines, that responsibility shifts to **Environments**. An Environment is an Azure DevOps resource that represents a deployment target (e.g., DevTest, Production). Protection rules — approvals, Azure Monitor checks, business hours, and more — are attached to the Environment itself, not the pipeline.

This means:

- **Environment owners** control *whether* a deployment is allowed to proceed.
- **Pipeline authors** control *what* gets deployed and *how*.
- Any YAML pipeline that targets a protected environment automatically inherits its checks.

There are several types of checks available on environments:

- **Approvals:** Require one or more users to manually approve before the stage runs.
- **Query Azure Monitor alerts:** Block deployment if active alerts exist in a specified resource group.
- **Invoke Azure Function:** Call a custom function and verify a successful response.
- **Invoke REST API:** Call any REST endpoint and evaluate the response.
- **Business hours:** Restrict deployments to specific time windows.

---

## Create and configure the team project

First, you'll create an Azure DevOps project for this lab.

1. In your browser, open your Azure DevOps organization.
2. Select **New Project**.
3. Give your project the name **eShopOnWeb**.
4. Leave other fields with defaults.
5. Select **Create**.

---

## Import the eShopOnWeb Git Repository

Next, you'll import the sample repository that contains the application code.

1. In your Azure DevOps organization, open the **eShopOnWeb** project.
2. Select **Repos > Files**.
3. Select **Import a Repository**, then select **Import**.
4. In the **Import a Git Repository** window, paste this URL: `https://github.com/MicrosoftLearning/eShopOnWeb.git`
5. Select **Import**.

The repository is organized this way:

| Folder | Contents |
|---|---|
| `.ado` | Azure DevOps YAML pipelines |
| `.devcontainer` | Setup to develop using containers |
| `infra` | Bicep & ARM infrastructure as code templates |
| `.github` | YAML GitHub workflow definitions |
| `src` | The .NET 8 website used in the lab scenarios |

6. Go to **Repos > Branches**.
7. Hover on the **main** branch, then select the ellipsis on the right.
8. Select **Set as default branch**.

---

## Create Azure Resources

You'll create two Azure web apps representing the DevTest and Production environments, plus Application Insights and an alert rule.

### Create two Azure web apps

1. From your lab computer, navigate to the **Azure Portal**.
2. Sign in with the user account that has the **Owner** role in your Azure subscription.
3. In the Azure portal, select the **Cloud Shell** icon (to the right of the search box).
4. If prompted to select either **Bash** or **PowerShell**, select **Bash**.

> **Note:** If this is your first time starting Cloud Shell and you see the "You have no storage mounted" message, select your subscription and select **Apply**.

5. From the Bash prompt, run this command to create a resource group (replace `<region>` with your preferred Azure region):

```bash
REGION='<region>'
RESOURCEGROUPNAME='az400m03l08-RG'
az group create -n $RESOURCEGROUPNAME -l $REGION
```

6. Create an App Service plan:

```bash
SERVICEPLANNAME='az400m03l08-sp1'
az appservice plan create -g $RESOURCEGROUPNAME -n $SERVICEPLANNAME --sku S1
```

7. Create two web apps with unique names:

```bash
SUFFIX=$RANDOM$RANDOM
az webapp create -g $RESOURCEGROUPNAME -p $SERVICEPLANNAME -n RGATES$SUFFIX-DevTest
az webapp create -g $RESOURCEGROUPNAME -p $SERVICEPLANNAME -n RGATES$SUFFIX-Prod
```

> **Note:** Record the name of the DevTest web app. You'll need it later.

> **Note:** If you get an error about the subscription not being registered to use namespace 'Microsoft.Web', run: `az provider register --namespace Microsoft.Web` and wait for registration to complete.

8. Wait for the provisioning process to complete and close the Cloud Shell pane.

### Configure an Application Insights resource

1. In the Azure portal, search for **Application Insights** and select it from the results.
2. On the Application Insights blade, select **+ Create**.
3. On the **Basics** tab, specify these settings:

| Setting | Value |
|---|---|
| Resource group | az400m03l08-RG |
| Name | the name of the DevTest web app from the previous task |
| Region | the same Azure region where you deployed the web apps |

4. Select **Review + create** and then select **Create**.
5. Wait for the provisioning process to complete.
6. Navigate to the resource group **az400m03l08-RG**.
7. In the list of resources, select the **DevTest** web app.
8. On the DevTest web app page, in the left menu under **Monitoring**, select **Application Insights**.
9. Select **Turn on Application Insights**.
10. In the **Change your resource** section, select **Select existing resource**.
11. Select the newly created Application Insight resource.
12. Select **Apply** and when prompted for confirmation, select **Yes**.
13. Wait until the change takes effect.

### Create a monitor alert rule

1. From the same Application Insights menu, select **View Application Insights Data**.
2. On the Application Insights resource blade, under **Monitoring**, select **Alerts**.
3. Select **Create > Alert rule**.
4. In the **Condition** section, select **See all signals**.
5. Type **Requests** and from the results, select **Failed Requests**.
6. In the **Condition** section, leave **Threshold** set to **Static** and validate these defaults:

| Setting | Value |
|---|---|
| Aggregation Type | Count |
| Operator | Greater Than |
| Unit | Count |

7. In the **Threshold value** textbox, type **0**.
8. Select **Next: Actions**.
9. Don't make changes in **Actions**, and define these parameters under **Details**:

| Setting | Value |
|---|---|
| Severity | 2 - Warning |
| Alert rule name | RGATESDevTest_FailedRequests |
| Advanced Options: Automatically resolve alerts | cleared |

10. Select **Review + Create**, then **Create**.
11. Wait for the alert rule to be created successfully.

> **Note:** Metric alert rules might take up to 10 minutes to activate.

---

## Create Azure DevOps Environments

In YAML pipelines, deployment targets are represented by **Environments**. You'll create two environments and configure protection checks on them.

### Create the DevTest and Production environments

1. In the Azure DevOps portal, open the **eShopOnWeb** project.
2. Navigate to **Pipelines > Environments**.
3. Select **Create environment**.
4. Enter the name **DevTest**, set **Resource** to **None**, and select **Create**.
5. On the **Environments** list, select **New environment** again.
6. Enter the name **Production**, set **Resource** to **None**, and select **Create**.

### Configure a pre-deployment approval on the DevTest environment

1. On the **Environments** list, select **DevTest** to open it.
2. You will see the DevTest environment page with two tabs: **Deployments** and **Approvals and checks**. The Deployments tab will show a "Get started!" message since no pipelines have targeted this environment yet.
3. Select the **Approvals and checks** tab.
4. Select **+ Add check** (or **Create** if this is the first check).
5. Select **Approvals** and select **Next**.
6. In the **Approvers** field, type and select your Azure DevOps account name.

> **Note:** In a real-life scenario, this should be a DevOps team name alias instead of your own name.

7. Select **Create**.

### Configure an Azure Monitor gate on the Production environment

The Azure Monitor check on the Production environment ensures that no active alerts exist for the DevTest web app *before* the Production deployment begins. This effectively replicates the classic "post-deployment gate" on DevTest — the check evaluates after DevTest succeeds but before Production starts.

1. Navigate back to **Pipelines > Environments**.
2. Select **Production** to open it.
3. Select the **Approvals and checks** tab.
4. Select **+ Add check**.
5. Select **Query Azure Monitor alerts** and select **Next**.
6. Configure the check with these settings:

| Setting | Value |
|---|---|
| Display name | Query Azure Monitor alerts |
| Azure subscription | Select the service connection representing your Azure subscription |
| Resource group | az400m03l08-RG |

7. Expand the **Advanced** section and configure:

| Setting | Value |
|---|---|
| Filter type | None |
| Severity | Sev0 (+4) |
| Time range | Past hour |
| Alert state | New (+1) |
| Monitor condition | Fired |

8. Expand **Control options** and configure:

| Setting | Value |
|---|---|
| Timeout (minutes) | 8 |
| Time between evaluations (minutes) | 5 |
| Linked Variable Group | *(leave empty)* |

> **Note:** The check will re-evaluate every 5 minutes. If active alerts still exist after 8 minutes, the check fails and the Production stage is rejected. These values mirror the original classic release gate timing — short enough for a lab, but in production scenarios you'd likely set a longer timeout.

9. Select **Create**.

---

## Create the Multi-Stage YAML Pipeline

You'll create a single YAML pipeline file that builds the application and deploys to both environments.

### Create the pipeline YAML file

1. In the Azure DevOps portal, navigate to **Repos > Files**.
2. In the `.ado` folder, select the **⋮** (more actions) menu and select **New > File**.
3. Name the file `eshoponweb-cd-gates.yml` and select **Create**.
4. Paste the following YAML content:

```yaml
resources:
  repositories:
    - repository: self
      trigger: none

stages:

# ── Build Stage ─────────────────────────────────────────
- stage: Build
  displayName: 'Build .Net Core Solution'
  jobs:
  - job: Build
    pool:
      vmImage: ubuntu-latest
    steps:
    - task: DotNetCoreCLI@2
      displayName: Restore
      inputs:
        command: 'restore'
        projects: '**/*.sln'
        feedsToUse: 'select'

    - task: DotNetCoreCLI@2
      displayName: Build
      inputs:
        command: 'build'
        projects: '**/*.sln'

    - task: DotNetCoreCLI@2
      displayName: Test
      inputs:
        command: 'test'
        projects: 'tests/UnitTests/*.csproj'

    - task: DotNetCoreCLI@2
      displayName: Publish
      inputs:
        command: 'publish'
        publishWebProjects: true
        arguments: '-o $(Build.ArtifactStagingDirectory)'

    - task: PublishPipelineArtifact@1
      displayName: 'Publish Artifacts ADO - Website'
      inputs:
        targetPath: '$(Build.ArtifactStagingDirectory)'
        artifact: 'Website'
        publishLocation: 'pipeline'

    - task: PublishPipelineArtifact@1
      displayName: 'Publish Artifacts ADO - Bicep'
      inputs:
        targetPath: '$(Build.SourcesDirectory)/infra/webapp.bicep'
        artifact: 'Bicep'
        publishLocation: 'pipeline'

# ── Deploy to DevTest ───────────────────────────────────
- stage: DeployDevTest
  displayName: 'Deploy to DevTest'
  dependsOn: Build
  jobs:
  - deployment: DeployDevTest
    displayName: 'Deploy to DevTest Web App'
    pool:
      vmImage: ubuntu-latest
    environment: 'DevTest'
    strategy:
      runOnce:
        deploy:
          steps:
          - download: current
            artifact: 'Website'

          - task: AzureWebApp@1
            displayName: 'Deploy Azure Web App'
            inputs:
              azureSubscription: '<YOUR-SERVICE-CONNECTION-NAME>'
              appType: 'webApp'
              appName: '<YOUR-DEVTEST-WEBAPP-NAME>'
              package: '$(Pipeline.Workspace)/Website/Web.zip'
              appSettings: >
                -UseOnlyInMemoryDatabase true
                -ASPNETCORE_ENVIRONMENT Development

# ── Deploy to Production ────────────────────────────────
- stage: DeployProduction
  displayName: 'Deploy to Production'
  dependsOn: DeployDevTest
  jobs:
  - deployment: DeployProduction
    displayName: 'Deploy to Production Web App'
    pool:
      vmImage: ubuntu-latest
    environment: 'Production'
    strategy:
      runOnce:
        deploy:
          steps:
          - download: current
            artifact: 'Website'

          - task: AzureWebApp@1
            displayName: 'Deploy Azure Web App'
            inputs:
              azureSubscription: '<YOUR-SERVICE-CONNECTION-NAME>'
              appType: 'webApp'
              appName: '<YOUR-PROD-WEBAPP-NAME>'
              package: '$(Pipeline.Workspace)/Website/Web.zip'
              appSettings: >
                -UseOnlyInMemoryDatabase true
                -ASPNETCORE_ENVIRONMENT Development
```

5. Replace `<YOUR-SERVICE-CONNECTION-NAME>` with the name of your Azure service connection (in both stages).
6. Replace `<YOUR-DEVTEST-WEBAPP-NAME>` with the name of your DevTest web app (e.g., `RGATES12345-DevTest`).
7. Replace `<YOUR-PROD-WEBAPP-NAME>` with the name of your Production web app (e.g., `RGATES12345-Prod`).
8. Select **Commit**, enter a commit message, and commit to the **main** branch.

> **Note:** The pipeline uses `trigger: none` on the self repository, so it will not run automatically when you commit. You will trigger it manually in the testing section.

### Register the pipeline

1. Navigate to **Pipelines > Pipelines**.
2. Select **New Pipeline** (or **Create Pipeline**).
3. On the **Where is your code?** pane, select **Azure Repos Git (YAML)**.
4. On the **Select a repository** pane, select **eShopOnWeb**.
5. On the **Configure your pipeline** pane, select **Existing Azure Pipelines YAML File**.
6. In the **Selecting an existing YAML file** blade, specify:
   - **Branch:** main
   - **Path:** `.ado/eshoponweb-cd-gates.yml`
7. Select **Continue**.
8. Review the YAML and select **Save** (not Run — you'll trigger it in the testing section).
9. Go to **Pipelines > Pipelines**, select the ellipsis on the newly created pipeline, and choose **Rename/move**.
10. Name it **eshoponweb-cd-gates** and select **Save**.

---

## Test the Pipeline and Release Gates

### Run the pipeline and approve DevTest

1. Navigate to **Pipelines > Pipelines** and select **eshoponweb-cd-gates**.
2. Select **Run pipeline**, accept defaults, and select **Run**.
3. Wait for the **Build** stage to complete successfully.
4. Once the Build stage finishes, the **Deploy to DevTest** stage will show a **Review** button because of the approval check you configured on the DevTest environment.
5. Select **Review**, then select **Approve** to allow the DevTest deployment.
6. Wait for the DevTest stage to complete successfully.

### Verify the deployment

1. Switch to the Azure portal and navigate to the **az400m03l08-RG** resource group.
2. Select the **DevTest** web app and select **Browse**.
3. Verify that the web page (eShopOnWeb) loads successfully.

### Generate alerts to trigger the gate

1. In your browser, navigate to the DevTest web app URL.
2. To simulate a failed request, append `/discount` to the URL (this page doesn't exist and will generate a 404 error).
3. Refresh this page several times to generate multiple failed request events.
4. In the Azure portal, search for **Application Insights** and select the DevTest Application Insights resource.
5. Under **Monitoring**, select **Alerts**.
6. There should be at least **1 new alert** with **Severity 2 - Warning** showing up.

> **Note:** If no alert shows up yet, wait another few minutes. Metric alert rules can take up to 10 minutes to activate.

### Observe the Production gate blocking deployment

1. Return to the Azure DevOps Portal and open the **eShopOnWeb** project.
2. Navigate to **Pipelines > Pipelines** and select **eshoponweb-cd-gates**.
3. Select **Run pipeline**, accept defaults, and select **Run**.
4. Wait for the **Build** stage to complete.
5. **Approve** the DevTest stage when prompted.
6. Wait for the **Deploy to DevTest** stage to complete.
7. Observe the **Deploy to Production** stage. Instead of immediately deploying, it will show a **Checks** status indicator. The **Query Azure Monitor alerts** check is now evaluating.
8. Select the stage to view the check status. You should see that the **Query Azure Monitor alerts** check has **failed** because there are active fired alerts in the `az400m03l08-RG` resource group.
9. The check will re-evaluate after 5 minutes. After the second evaluation, it should also fail.

> **Note:** This is expected behavior. Because there is an active Application Insights alert triggered for the DevTest web app, the Azure Monitor gate blocks the deployment to Production. This prevents unhealthy code from being promoted.

10. After 8 minutes (the timeout you configured), the check will expire and the Production stage will be marked as **Rejected**.

> **Note:** To see a successful gate, navigate to the Azure portal, open the alert in Azure Monitor, and select **Change state > Closed**. The next evaluation cycle should then pass, allowing the Production deployment to proceed.

---

## Clean up resources

Remember to delete the resources created in the Azure portal to avoid unnecessary charges:

1. In the Azure portal, navigate to the **az400m03l08-RG** resource group.
2. Select **Delete resource group**.
3. Type the resource group name to confirm deletion.
4. Select **Delete**.

---

## Summary

In this lab, you configured a multi-stage YAML pipeline with environment-based deployment gates. You learned how to:

- ✅ **Create a multi-stage YAML pipeline** that builds, publishes, and deploys to multiple environments in a single definition
- ✅ **Create Azure DevOps Environments** to represent deployment targets (DevTest, Production)
- ✅ **Configure manual approvals** on an environment to gate deployments
- ✅ **Configure Azure Monitor alert checks** on an environment to automatically block promotion when the application is unhealthy
- ✅ **Observe gate evaluation** in action — seeing checks pass or fail based on real application health data

### Key Differences from Classic Release Pipelines

| Concept | Classic Designer | YAML Multi-Stage |
|---|---|---|
| Pipeline definition | Separate CI build + Release pipeline | Single `.yml` file with stages |
| Deployment target | Stages in the Release UI | **Environments** in Pipelines > Environments |
| Approvals | Configured on each stage | Configured as checks on the **Environment** |
| Gates (e.g., Azure Monitor) | Pre/post-deployment gates on a stage | **Approvals and checks** on the **Environment** |
| Artifacts | Linked artifact sources | `PublishPipelineArtifact` / `DownloadPipelineArtifact` tasks |
| Version control | Only CI pipeline is in source control | **Entire pipeline** is in source control |
| Protection ownership | Pipeline author configures gates | **Environment owner** configures checks |

Environment-based checks ensure that *any* pipeline targeting a protected environment must pass the same quality gates, providing consistent, centralized deployment governance across your organization.
