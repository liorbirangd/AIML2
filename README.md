# AIML - BCS 2

**Date:** 21.01.2025

## Purpose of Project
This project utilizes deep reinforcement learning algorithms within a real-time 3D soccer game environment to demonstrate the capabilities and implementation of AI strategies.

## System Requirements
- **Unity Version**: 2022.3.45f1

## Environment Overview
The simulation is set in a 3D soccer arena where two teams compete. The setup includes a field, two goals, a soccer ball, and four agents (two per team). The objective for each team is to score goals against the opposite team.

## Agents
Each agent in this project is equipped with several scripts that enable training and execution:
- **AgentSoccer**: This class extends the ML-Agents' `Agent` class, serving as the main connector between the agent and the training process. It manages reward settings and action executions.
  - **Methods**:
    - `Initialize()`: Invoked when the agent is first enabled to initialize necessary variables and establish links with other objects.
    - `OnActionReceived(ActionBuffers actionBuffers)`: Triggered by model output, this method processes the received actions. The agent can perform three discrete actions: move forward, move sideways, and rotate. Each action accepts values: 0 (no action), 1 (move/rotate right or forward), or 2 (move/rotate left or backward).
    - `OnEpisodeBegin()`: Called at the start of each training, reset the agent to prepare for a new episode.
    - `Heuristic(in ActionBuffers actionsOut)`: Used for manual control by a human, bypassing the model when testing or demonstrating.
    - `CollectObservations(VectorSensor sensor)`: Collects environmental data at every timestep. This method gathers observations from Sensor components that the agent has. observations from a RaycastPerception3DSensor is gathered automatically not through this method.
- **BehaviorParameters**: Accompanies the Agent class to adjust parameters for training.
  - **Variables**:
    - Behavior Name: Must match the behavior name specified in the .yaml configuration file.
    - Vector Observations: Sets the number of observations the agent collects, this value is calculated and set by the AgentSoccer script in the `InitializeSensors()` method.
    - Actions: Defines the quantity and type (discrete/continuous) of actions the agent can perform.
    - Model: Connects the trained models post-training to the agent within Unity.
    - TeamID: Essential for team training, ensuring each agent is associated with the correct team to qualify for team-specific rewards, such as scoring goals.

## Sensors
Agents gather data from the environment using sensors, which are crucial for navigating and interacting within the game space:
### Exsisting Sensors
- **___RayPerceptionSensor**: Positioned on the agent, this component emits 11 forward-pointing rays to detect objects like walls, the ball, and other agents. This sensor uses the `Raycast'PerceptionRotatingSnesor` which inherits from `RayPerceptionSensorComponent3D`, it performs the observation collection automatically and does not use the `CollectObservetions` method.
- **___RayPerceptionSensorReverse**: This sensor sends out 3 backward-pointing rays, enhancing the agent's awareness of its surroundings by detecting objects from behind. This sensor uses `RayPerceptionSensorComponent3D`, and as it is an unrealistic it has been disabled.
- **___HearingSensor**: This sensor employs a sphere hitbox around the agent that detects other objects that move. based on their position and distance, the agent learns an area in which the object is located. This sensor emulates hearing, and uses the `HearingSensorController` script.
- **___AwarnessSensor**: This sensor is composed of 3 subcomponents, a sound sensor, a sight sensor, and a proximity sensor, When an object is detected by one of those subsensors, the agent increases its awarness score associated with the object, and one the awarness is above a threshold it updates the last known position where the detected object was.
### Adding New Sensors
- Create the sensor script and attach it to a new object under the agents' object.
- In the sensor script, implement an `AddObservations(VectorSensor sensor)` method that adds the relevant observations.
- In the `AgentSoccer` class, create an object that will reference the sensor, under the `InitializeSensors()` method, fetch the reference to that sensor, and add a calculation to account for the number of observations that sensor would providde.
- In the `AgentSoccer` class, under the `CollectObservations(VectorSensor sensor)` method, call the `AddObservations(VectorSensor sensor)` of the new sensor if it exsits.
### Enabling/Disabling Sensors
In order to enable or disable a sensor, go to the field prefab in unity, under each of the 4 agents, select the sensor and enable/disable its object.

## Rewards
Agents must receive rewards for encouraged actions to facilitate effective training. To facilitae a modular reward system, we introduced the `RewardManager` class. each agent creates a `RewardManager` object, and uses it as the subject in an Observer pattern. The manager object contains multiple UnityEvents that areenvoked when different triggers happen to the agent, such as colliding with the ball. For each reward component, we created a seperate class that controls the checks and calculations regarding that reward, and they are acting as listeners to the relevant events in the RewardManager. In ordere to add new rewards, or turn off existing rewards, they need to be initialized in the `InitializeRewardComponents()` of the `AgentSoccer`. having componnents missing is not a problem as the events would still trigger but without listeners.
### Existing Reward Components
- **Existantial**: Adds an existantial reward or penalty to goalies and strikers repectively every time the model takes an action.
- **Ball Touch**: Adds a reward to the agent whenver it touchecs the ball.
- **Scoring**: This is a unique reward/penalty that is added as group reward from the `SoccerEnvironmentController` class to the scoring and loosing team when a goal is scored. 

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
   mlagents-learn SoccerTwos.yaml --run-id=<training session name> --train
Once prompted, start the Unity environment in the SoccerTwos scene.
- To pause a trainign, you can press CTRL+C in the anaconda prompt window, after which you may resume the training with the command
   ```bash
   mlagents-learn SoccerTwos.yaml --run-id=<training session name> --resume
### Viewing Results and Data Analysis
1. Open a new Anaconda Prompt, activate the environment, and go to the training directory:
  ```bash
  tensorboard --logdir <results folder name>
  ```
    
2. Access TensorBoard through the URL provided in the CLI.
3. Select the graphs that you are interested in, and download the data for the relevant trainigns.
4. Aggregate the data in one file with the first column being all the steps, and the next columns containig the data for the metrics of the groups you are trying to compare.
5. Save the file in the **Statistical Analysis** folder in the repository.
6. Open the **StatisticalAnalysis.py** file in that folder
7. In main set the name of the file to the name you used for the data file, set the label for the experiment, and the minimize_parameter to true or false depending on the metric you are analyzing (if the goal is to minimize or maximize it).

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

