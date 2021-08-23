# DO THIS BEFORE RUNNING THE APPLICATION

# Create a Kaggle account and navigate to the following site:
#   https://www.kaggle.com/maxhorowitz/nflplaybyplay2009to2016

# Download the following file into this folder:
#   NFL Play by Play 2009-2018 (v5).csv

# Install the C1DataEngine command line tool:
dotnet tool install -g C1.DataEngine.Tool

# Run this command to import the data:
c1dataengine table Plays --provider csv --connection "URI=NFL Play by Play 2009-2018 (v5).csv"
