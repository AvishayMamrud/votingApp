#!/bin/bash

root="Services/ResultsService/ResultsProj"

# Define all folders
folders=(
  "$root/Application/Interfaces"
  "$root/Application/Handlers"
  "$root/Business/Logic"
  "$root/Business/Models"
  "$root/Data/DAL"
  "$root/Data/DTOs"
  "$root/Infrastructure/Messaging"
  "$root/Controllers"
)

# Create folders
for folder in "${folders[@]}"; do
  mkdir -p "$folder"
done

# Create empty .cs files
declare -A files=(
  ["$root/Application/Interfaces/IResultsUpdateHandler.cs"]=""
  ["$root/Application/Interfaces/IGatewayPushPublisher.cs"]=""
  ["$root/Application/Interfaces/IDataAccess.cs"]=""
  ["$root/Application/Handlers/ResultsUpdateHandler.cs"]=""
  ["$root/Business/Logic/ResultsLogic.cs"]=""
  ["$root/Business/Models/SurveyResult.cs"]=""
  ["$root/Data/DAL/DataAccess.cs"]=""
  ["$root/Data/DAL/ORMEntities.cs"]=""
  ["$root/Data/DTOs/VoteDTO.cs"]=""
  ["$root/Data/DTOs/SurveyDTO.cs"]=""
  ["$root/Infrastructure/Messaging/SurveyResultsListener.cs"]=""
  ["$root/Infrastructure/Messaging/VotesListener.cs"]=""
  ["$root/Infrastructure/Messaging/GatewayPushPublisher.cs"]=""
  ["$root/Controllers/ResultsController.cs"]=""
  ["$root/Program.cs"]=""
  ["$root/ResultsService.csproj"]=""
)

# Create files
for file in "${!files[@]}"; do
  touch "$file"
done

echo "✅ ResultsService skeleton created!"
