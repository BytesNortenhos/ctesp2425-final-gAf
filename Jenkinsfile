pipeline {
    agent any

    environment {
        DOCKER_IMAGE = 'ctesp2425-final-gaf'
        SONAR_PROJECT_KEY = 'ctesp2425-final-gaf'
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = 'true'
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

                                # Run SonarQube analysis
                                dotnet sonarscanner begin /k:"reservation-api" /d:sonar.host.url="http://sonarqube:9000/" /d:sonar.login="sqp_55efbeed057d640bc44e67cec936fbb9532cd530"
                                dotnet build ctesp2425-final-gAf/ctesp2425-final-gAf.csproj --no-restore
                                dotnet sonarscanner end /d:sonar.login="sqp_8b3fe0b6a7aa8760fe8f98ea7191f30e96c2638a"
                            '''
                        } else {
                            bat '''
                                dotnet tool install --global dotnet-sonarscanner || true
                                dotnet sonarscanner begin /k:"reservation-api" /d:sonar.host.url="http://sonarqube:9000/" /d:sonar.login="sqp_55efbeed057d640bc44e67cec936fbb9532cd530"
                                dotnet build ctesp2425-final-gAf/ctesp2425-final-gAf.csproj --no-restore
                                dotnet sonarscanner end /d:sonar.login="sqp_8b3fe0b6a7aa8760fe8f98ea7191f30e96c2638a"
                            '''
                        }
                    }
                }
            }
        }

        stage('Docker Build') {
            steps {
                script {
                    if (isUnix()) {
                        sh "docker build -t ${DOCKER_IMAGE} -f ctesp2425-final-gAf/Dockerfile ctesp2425-final-gAf"
                    } else {
                        bat "docker build -t ${DOCKER_IMAGE} -f ctesp2425-final-gAf/Dockerfile ctesp2425-final-gAf"
                    }
                }
            }
        }

        stage('Deploy') {
            steps {
                script {
                    if (isUnix()) {
                        sh """
                            docker stop ${DOCKER_IMAGE} || true
                            docker rm ${DOCKER_IMAGE} || true
                            docker run -d --name ${DOCKER_IMAGE} -p 8050:8080 ${DOCKER_IMAGE}
                        """
                    } else {
                        bat """
                            docker stop ${DOCKER_IMAGE} || true
                            docker rm ${DOCKER_IMAGE} || true
                            docker run -d --name ${DOCKER_IMAGE} -p 8050:8080 ${DOCKER_IMAGE}
                        """
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