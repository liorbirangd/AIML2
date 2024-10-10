# AIML - BCS 2

**Date:** 09.10.2024

## Purpose of Project
This project utilizes deep reinforcement learning algorithms within a real-time 3D soccer game environment to demonstrate the capabilities and implementation of AI strategies.

## System Requirements
- **Unity Version**: 2022.3.45f1

## Environment Overview
The simulation is set in a 3D soccer arena where two teams compete. The setup includes a field, two goals, a soccer ball, and four agents (two per team). The objective for each team is to score goals against the opposition.

## Agents
Each agent in this project is equipped with several scripts that enable training and execution:
- **AgentSoccer**: This class extends the ML-Agents' `Agent` class, serving as the main connector between the agent and the training process. It manages reward settings and action executions.
  - **Methods**:
    - `Initialize()`: Invoked when the agent is first enabled to initialize necessary variables and establish links with other objects.
    - `OnActionReceived(ActionBuffers actionBuffers)`: Triggered by model output, this method processes the received actions. The agent can perform three discrete actions: move forward, move sideways, and rotate. Each action accepts values: 0 (no action), 1 (move/rotate right or forward), or 2 (move/rotate left or backward).
    - `OnEpisodeBegin()`: Called at the start of each training, reset the agent to prepare for a new episode.
    - `Heuristic(in ActionBuffers actionsOut)`: Used for manual control by a human, bypassing the model when testing or demonstrating.
    - `CollectObservations(VectorSensor sensor)`: Collects environmental data at every timestep. Although the method isn’t overridden in this project, it still gathers observations from the existing sensors on the agent and its children.
    - **Rewards**: Agents must receive rewards for encouraged actions to facilitate effective training. The `AddReward(float)` method awards rewards when agents hit the ball or when their team scores a goal. Existential rewards or penalties are also assigned based on the agent's role as either a Goalie or Striker.
- **BehaviorParameters**: Accompanies the Agent class to adjust parameters for training.
  - **Variables**:
    - Behavior Name: Must match the behavior name specified in the .yaml configuration file.
    - Vector Observations: Sets the number of observations the agent collects, which is automatically calculated and should remain at 0 in this project since the `CollectObservations` method isn't overridden.
    - Actions: Defines the quantity and type (discrete/continuous) of actions the agent can perform.
    - Model: Connects the trained models post-training to the agent within Unity.
    - TeamID: Essential for team training, ensuring each agent is associated with the correct team to qualify for team-specific rewards, such as scoring goals.

## Sensors
Agents gather data from the environment using built-in sensors, which are crucial for navigating and interacting within the game space:
- **___RayPerceptionSensor**: Positioned on the agent, this component emits 11 forward-pointing rays to detect objects like walls, the ball, and other agents.
- **___RayPerceptionSensorReverse**: Located on a child of the agent, this sensor sends out 3 backward-pointing rays, enhancing the agent's awareness of its surroundings by detecting objects from behind.

## Download Unity Project
1. Download the Unity Engine Hub from [UnityHub Download](https://unity.com/download).
2. In the UnityHub go to the installation tab, and install the Unity version: 2022.3.45f1.
3. Download GitHub Desktop (or use the terminal) from [GitHub Desktop Download](https://desktop.github.com/download/).
4. Clone this repository to your computer.
5. Open UnityHub, "click Add>Add Project From Disc" and search for the repository. Make sure to open the  "AIML-Project" folder.
6. Inside the Unity Editor, the main scene is in the folder "Assets>Soccer>Scenes>SoccerTwos"

## Python Environment Setup
To prepare the training environment:
1. Download and install Anaconda from [Anaconda Download](https://www.anaconda.com/download/success).
2. Open Anaconda Prompt.
3. Create a new environment:
   ```bash
   conda create -n mlagents python=3.8
  Confirm with 'y' when prompted.

4. Activate the new environment:
   ```bash
   conda activate mlagents
5. Install dependencies:
   ```bash
   pip install mlagents==0.28.0 cattrs==1.5.0 protobuf==3.20.* torch==1.10.0+cpu torchvision==0.11.1+cpu torchaudio==0.10.0+cpu -f https://download.pytorch.org/whl/cpu/torch_stable.html numpy==1.19.5 six
   python -c "import torch; print(torch.__version__)"
6. Verify installation:
   ```bash
   mlagents-learn -h

## How to Train the Agents
1. Open Anaconda Prompt and activate the ML-Agents environment:
   ```bash
   conda activate mlagents
2. Navigate to the directory containing the SoccerTwos.yaml configuration file:
   ```bash
   cd <path to folder>
3. Start the training:
   ```bash
   mlagents-learn SoccerTwos.yaml --run-id=<training session name>
Once prompted, start the Unity environment in the SoccerTwos scene.

### Viewing Results
- Open a new Anaconda Prompt, activate the environment, and go to the training directory:
  ```bash
  tensorboard --logdir results
- Access TensorBoard through the URL provided in the CLI.

## ML-Agents Library
This project makes use of the ML-Agents toolkit, an open-source project from Unity Technologies aimed at enabling games and simulations to serve as environments for training intelligent agents. For more details and licensing information, visit the [ML-Agents GitHub repository](https://github.com/Unity-Technologies/ml-agents).

## Licensing
This project is licensed under the Apache License, Version 2.0. You may obtain a copy of the license at: [Apache License, Version 2.0](http://www.apache.org/licenses/LICENSE-2.0).

## Authors
- Rana Eltahir
- Natalia Hadjisoteriou
- Styliani Mikelli
- Laura van Rooij
- Aleksandar Stoychev
- Floris Voogt
- Lior Biran

## Acknowledgements
Special thanks to the Department of Advanced Computing Sciences at Maastricht University for their continuous support and guidance.

