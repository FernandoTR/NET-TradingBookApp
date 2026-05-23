#!/usr/bin/env bash

COMMIT_MSG="$1"

if [ -z "$COMMIT_MSG" ]; then
  echo "Error: commit message is required"
  exit 1
fi

echo "$COMMIT_MSG" | grep -E '^(feat|fix|docs|style|refactor|perf|test|build|ci|chore|revert)(\(.+\))?(!)?: .+$'

if [ $? -ne 0 ]; then
  echo "Invalid Conventional Commit format"
  exit 1
fi

echo "Commit message valid"
