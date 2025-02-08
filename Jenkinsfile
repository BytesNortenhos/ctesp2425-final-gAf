pipeline {
    agent any

    environment {
        DOCKER_IMAGE = 'ctesp2425-final-gaf'
        SONAR_PROJECT_KEY = 'ctesp2425-final-gaf'
    }

    tools {
        dotnetsdk 'dotnet-sdk'
    }

    stages {
        stage('Install libicu (Unix Only)') {
            when {
                expression { isUnix() } // Only run this stage on Unix-like systems
            }
            steps {
                sh '''
                    # Install libicu based on the Linux distribution
                    if command -v apt-get &> /dev/null; then
                        apt-get update || true
                        apt-get install -y libicu-dev || true
                    elif command -v yum &> /dev/null; then
                        yum install -y libicu || true
                    elif command -v apk &> /dev/null; then
                        apk add icu-libs || true
                    elif command -v dnf &> /dev/null; then
                        dnf install -y libicu || true
                    else
                        echo "Unsupported package manager. Please install libicu manually."
                        exit 1
                    fi
                '''
            }
        }

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                script {
                    if (isUnix()) {
                        sh 'dotnet restore ctesp2425-final-gAf/ctesp2425-final-gAf.csproj'
                    } else {
                        bat 'dotnet restore ctesp2425-final-gAf/ctesp2425-final-gAf.csproj'
                    }
                }
            }
        }

        stage('Build') {
            steps {
                script {
                    if (isUnix()) {
                        sh 'dotnet build ctesp2425-final-gAf/ctesp2425-final-gAf.csproj --configuration Release --no-restore'
                    } else {
                        bat 'dotnet build ctesp2425-final-gAf/ctesp2425-final-gAf.csproj --configuration Release --no-restore'
                    }
                }
            }
        }

        stage('Restore XUnit Test') {
            steps {
                script {
                    if (isUnix()) {
                        sh 'dotnet restore XUnit_Test/X