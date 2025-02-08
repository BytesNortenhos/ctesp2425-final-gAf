ipeline {
    agent any

    environment {
        DOCKER_IMAGE = 'ctesp2425-final-gaf'
        SONAR_PROJECT_KEY = 'ctesp2425-final-gaf'
    }

    tools {
        dotnetsdk 'dotnet-sdk'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                sh 'dotnet restore ctesp2425-final-gAf.csproj'
            }
        }

        stage('Build') {
            steps {
                sh 'dotnet build ctesp2425-final-gAf.csproj --configuration Release --no-restore'
            }
        }

        stage('Test') {
            steps {
                sh 'dotnet test XUnit_Test/XUnit_Test.csproj --no-restore --verbosity normal /p:CollectCoverage=true /p:CoverletOutputFormat=opencover'
            }
        }

        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv('SonarQube') {
                    sh """
                        dotnet tool install --global dotnet-sonarscanner || true
                        dotnet sonarscanner begin /k:"${SONAR_PROJECT_KEY}" /d:sonar.host.url="http://sonarqube:9000"
                        dotnet build ctesp2425-final-gAf.csproj --no-restore
                        dotnet sonarscanner end
                    """
                }
            }
        }

        stage('Docker Build') {
            steps {
                sh "docker build -t ${DOCKER_IMAGE} ."
            }
        }

        stage('Deploy') {
            steps {
                sh """
                    docker stop ${DOCKER_IMAGE} || true
                    docker rm ${DOCKER_IMAGE} || true
                    docker run -d --name ${DOCKER_IMAGE} -p 8050:8080 ${DOCKER_IMAGE}
                """
            }
        }
    }

    post {
        always {
            cleanWs()
        }
    }
}