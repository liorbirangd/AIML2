# AIML - BCS 2

02.10.2024

### PURPOSE OF PROJECT

This project aims to apply deep reinforcement learning algorithms in a real-time 3D soccer game environment. 


### Environment Overview
The environment of this project is composed of a 3D soccer game, the game has a field, goals, a ball, and 4 agents on 2 teams. 
The 2 teams play against each other to get the ball in the opposing team's goal.

### Agents
Each agent object has a few scripts that let us run and train them:
- **AgentSoccer** The AgentSoccer class extends the Agent class provided by the MLAgents packages.
- **BehaviorParameters** 
- **DecisionRequester** This component asks the agent's model to perform an action.

### Sensors
The agents collect observations from the environment as input, these observations are collected via sensors.
The sensors of the agent consist of raycasts originating from the agent object and reporting on hitting different objects such as the walls, the ball, and other agents.
- **___RayPerceptionSensor** The first raycast object, a component on the agent object, has rays pointed forward with a total of 11 rays.
- **___RayPerceptionSensorReverse** The second set of raycasts, a component on a child of the agent, has rays pointed backwards with a total of 3 rays.

### Python Environment Setup
In order to train the agents, we need to set up a Python environment with the mlagents libraries. To do that follow the following steps:
- Download Anaconda Prompt from "https://www.anaconda.com/download/success"
- Open Anaconda Prompt
- Create a new  environment using the following command:
  conda create -n mlagents python=3.8
  Press y to accept when prompted by the CLI
- Activate the environment
  conda activate mlagents
- Install all necessary libraries, run the following commands one by line:
  pip install mlagents==0.28.0 cattrs==1.5.0
  pip install protobuf==3.20.*
  pip install torch==1.10.0+cpu torchvision==0.11.1+cpu torchaudio==0.10.0+cpu -f https://download.pytorch.org/whl/cpu/torch_stable.html
  python -c "import torch; print(torch.__version__)"
  pip install numpy==1.19.5
  pip install six
- Check that the installation works properly by running the command:
  mlagents-learn -h

### How To Train The Agents
After the Python environment is set up, we want to train the agents:
- Open Anaconda Prompt
- Activate the environment using:
  conda activate mlagents
- Go into the folder with the .yaml file, this file is in the config/poca folder under this project. Use the command:
  cd \<path to folder\>
- write the following command:
  mlagents-learn SoccerTwos.yaml --run-id=\<name of training\>
  the SoccersTwos.yaml file is a file with a parameters profile for the training process.
- Wait until a message in the CLI says the training process is ready to start.
- In Unity press play to start the game in the SoccerTwos scene.
**Viewing the results**
- In order to view the training process results, open a new Anaconda Prompt window, activate the environment and go to the same folder as the one opened for training.
- Type the following command:
tensorboard --logdir results
- Open a web browser and go to the URL printed in the CLI

### AUTHORS

- Rana Eltahir
- Natalia Hadjisoteriou
- Styliani Mikelli
- Laura van Rooij
- Aleksandar Stoychev
- Floris Voogt
- Lior Biran

### Acknowledgements

We extend our gratitude to the Department of Advanced Computing Sciences at Maastricht University for their support and
guidance throughout the development of this project.
