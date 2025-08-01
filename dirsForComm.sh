#!/bin/bash

set -e

services=("UsersService" "SurveysService" "VotesService" "ResultsService")

echo "Adding service projects to solution..."
for service in "${services[@]}"; do
  CSPROJ="Services/$service/${service%Service}Proj/${service%Service}Proj.csproj"
  echo "Processing $service..."
  if [ -f "$CSPROJ" ]; then
    dotnet sln VotingApp.sln add "$CSPROJ"
    echo "✅ Added $service to solution."
  else
    echo "⚠️  Skipping $service — .csproj not found."
  fi
done

echo "✅ Done. Your solution is ready."
