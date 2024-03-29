# Microservice Structured Workflow Concurrent Anomaly Detection

This .NET 8 web service detects concurrent artifact anomalies in microservice workflows, leveraging SP-trees for efficient analysis. Designed to integrate with Camunda Modeler, this tool enhances microservice architecture reliability by early anomaly identification and resolution.

## Features

- **Concurrent Anomaly Detection**: Leverages SP-tree structures for accurate anomaly identification.
- **.NET 8**: Demonstrates a robust, scalable service for anomaly detection.
- **Categorization**: Separates anomalies into errors or warnings, aiding prioritization.
- **Camunda Modeler Integration**: Features a plugin for intuitive, real-time anomaly feedback.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/)
- [Camunda Modeler](https://camunda.com/download/modeler/)

### Setup

1. Clone the repository and navigate to the service directory:
    ```bash
    git clone https://github.com/yourgithubusername/microservice-workflow-anomaly-detection.git
    cd microservice-workflow-anomaly-detection/BPMN-Anomaly-Inspector
    ```

2. Run the service:
    ```bash
    dotnet BPMN-Anomaly-Inspector.dll
    ```

3. For the modified Camunda Modeler:
    ```bash
    cd camunda-modeler-AnomalyInspector/camunda-modeler-develop
    npm install
    npm run dev
    ```

## Workflow Analysis

- **Manual Submission**:
    Use `curl` to submit BPMN files:
    ```bash
    curl --location 'http://localhost:5159/FileUpload/UploadBPMN?isPrevApproach=false' --form 'file=@"/path/to/your/bpmn/file.bpmn"'
    ```

- **Automatic Detection via Camunda Modeler**:
    Click **"Deploy"** in the Modeler to send your BPMN file for analysis, viewing anomaly detection results directly in the interface.

## Advantages

This method streamlines the workflow validation process, offering immediate feedback and ensuring workflow reliability by leveraging SP-tree based anomaly detection, directly integrated into the design tool for microservice architectures.
