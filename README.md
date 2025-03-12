# Addressable Template

## Requirements
- Unity Addressables version **1.18.19** is required for this template.

## Setup Steps

### 1. Create Addressable Groups
Set up Addressable Groups in Unity.

![Create Groups](https://github.com/SinlessDevil/Addressable_Template/blob/main/src_images/src%20(2).png)

### 2. Configure Load and Build Paths
For each group, specify the **Load Path** and **Build Path** accordingly:

![Load & Build Paths](https://github.com/SinlessDevil/Addressable_Template/blob/main/src_images/src%20(1).png)

### 3. Define Group Loading Rules
Determine when each group should be loaded based on your project's needs:

![Set Load Rules](https://github.com/SinlessDevil/Addressable_Template/blob/main/src_images/src%20(3).png)

### 4. Specify Required Groups (Optional)
If necessary, define which groups must be loaded explicitly:

![Specify Groups](https://github.com/SinlessDevil/Addressable_Template/blob/main/src_images/src%20(4).png)

### 5. Bundle Assembly
Ensure all necessary assets are bundled correctly:

![Bundle Assembly](https://github.com/SinlessDevil/Addressable_Template/blob/main/src_images/src%20(5).png)

### 6. Remote Hosting Setup (For Remote Builds)
If using **local development**, you can skip this step.
For **remote builds**, a server or storage solution is required. You can use **Google Cloud Storage** for testing.
Upload the **bundles folder** to the selected storage location:

![Upload to Server](https://github.com/SinlessDevil/Addressable_Template/blob/main/src_images/src%20(6).png)

### 7. Configure Remote Load Path
- For **Addressables version 1.18.19**, use **gsutil URL**.
- For **version 1.20 and above**, use **Public URL**:

![Configure Load Path](https://github.com/SinlessDevil/Addressable_Template/blob/main/src_images/src%20(7).png)

### 8. Create a New Profile
Create a new Addressables profile and modify the **Remote Load Path** accordingly:

![New Profile Setup](https://github.com/SinlessDevil/Addressable_Template/blob/main/src_images/src%20(8).png)

---
This setup ensures that your Addressable assets are correctly managed and loaded in Unity, whether locally or from a remote server.

