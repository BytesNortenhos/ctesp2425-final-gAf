pipeline {
    agent any

    environment {
        DOCKER_IMAGE = 'ctesp2425-final-gaf'
        DOCKER_HUB_REPO = 'robertovalentee/ctesp2425-final-gaf' // Replace with your Docker Hub repository
        SONAR_PROJECT_KEY = 'reservation-api'
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = 'true'
        SONAR_TOKEN = 'squ_612ea39c5b07a060668b3b191e25af84e32b191f' // Hardcoded SonarQube token
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
                        sh 'dotnet restore XUnit_Test/XUnit_Test.csproj'
                    } else {
                        bat 'dotnet restore XUnit_Test/XUnit_Test.csproj'
                    }
                }
            }
        }

        stage('Test') {
            steps {
                script {
                    if (isUnix()) {
                        sh 'dotnet test XUnit_Test/XUnit_Test.csproj --no-restore --verbosity normal /p:CollectCoverage=true /p:CoverletOutputFormat=opencover'
                    } else {
                        bat 'dotnet test XUnit_Test/XUnit_Test.csproj --no-restore --verbosity normal /p:CollectCoverage=true /p:CoverletOutputFormat=opencover'
                    }
                }
            }
        }

        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv('SonarQube') {
                    script {
                        if (isUnix()) {
                            sh '''
                                # Install dotnet-sonarscanner
                                dotnet tool install --global dotnet-sonarscanner || true

                                # Add .NET tools directory to PATH
                                export PATH="$PATH:/var/jenkins_home/.dotnet/tools"

                                # Verify dotnet-sonarscanner is available
                                which dotnet-sonarscanner || echo "dotnet-sonarscanner not found"
                        
                                # Begin SonarQube analysis with explicit server URL and authentication
                                dotnet sonarscanner begin \
                                    /k:"${SONAR_PROJECT_KEY}" \
                                    /d:sonar.host.url="http://sonarqube:9000" \
                                    /d:sonar.login="${SONAR_TOKEN}" \
                                    /d:sonar.qualitygate.wait=true
                            
                                dotnet build ctesp2425-final-gAf/ctesp2425-final-gAf.csproj --no-restore
                                dotnet sonarscanner end /d:sonar.login="${SONAR_TOKEN}"
                            '''
                        } else {
                            bat '''
                                dotnet tool install --global dotnet-sonarscanner || true
                                dotnet sonarscanner begin /k:"${SONAR_PROJECT_KEY}" /d:sonar.host.url="http://sonarqube:9000/" /d:sonar.login="${SONAR_TOKEN}"
                                dotnet build ctesp2425-final-gAf/ctesp2425-final-gAf.csproj --no-restore
                                dotnet sonarscanner end /d:sonar.login="${SONAR_TOKEN}"
                            '''
                        }
                    } 
                }
            }
        }

        stage('Docker Build') {
            steps {
                script {
                    docker.withServer('unix:///var/run/docker.sock') {
                        if (isUnix()) {
                            sh "docker build -t ${DOCKER_IMAGE} -f ctesp2425-final-gAf/Dockerfile ctesp2425-final-gAf"
                        } else {
                            bat "docker build -t ${DOCKER_IMAGE} -f ctesp2425-final-gAf/Dockerfile ctesp2425-final-gAf"
                        }
                    }
                }
            }
        }

        stage('Push Docker Image to Docker Hub') {
            steps {
                script {
                    docker.withServer('unix:///var/run/docker.sock') {
                        // Use Jenkins Docker settings for registry URL and credentials
                        docker.withRegistry('https://index.docker.io/v1/', 'docker-hub-credentials') {
                            if (isUnix()) {
                                sh "docker tag ${DOCKER_IMAGE} ${DOCKER_HUB_REPO}:latest"
                                sh "docker push ${DOCKER_HUB_REPO}:latest"
                            } else {
                                bat "docker tag ${DOCKER_IMAGE} ${DOCKER_HUB_REPO}:latest"
                                bat "docker push ${DOCKER_HUB_REPO}:latest"
                            }
                        }
                    }
                }
            }
        }

        stage('Deploy from Docker Hub') {
            steps {
                script {
                    docker.withServer('unix:///var/run/docker.sock') {
                        if (isUnix()) {
                            sh """
                                # Stop and remove the existing container
                                docker stop ${DOCKER_IMAGE} || true
                                docker rm ${DOCKER_IMAGE} || true

                                # Pull the latest image from Docker Hub
                                docker pull ${DOCKER_HUB_REPO}:latest

                                # Run the new container
                                docker run -d --name ${DOCKER_IMAGE} -p 8050:8080 ${DOCKER_HUB_REPO}:latest
                            """
                        } else {
                            bat """
                                docker stop ${DOCKER_IMAGE} || true
                                docker rm ${DOCKER_IMAGE} || true
                                docker pull ${DOCKER_HUB_REPO}:latest
                                docker run -d --name ${DOCKER_IMAGE} -p 8050:8080 ${DOCKER_HUB_REPO}:latest
                            """
                        }
                    }
                }
            }
        }
    }

    post {
        always {
            cleanWs() 
        }
    }
}