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
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                bat 'dotnet restore ctesp2425-final-gAf/ctesp2425-final-gAf.csproj'
            }
        }

        stage('Build') {
            steps {
                bat 'dotnet build ctesp2425-final-gAf/ctesp2425-final-gAf.csproj --configuration Release --no-restore'
            }
        }

        stage('Restore XUnit Test') {
            steps {
                // Adiciona a restauração dos pacotes NuGet para o projeto de testes
                bat 'dotnet restore XUnit_Test/XUnit_Test.csproj'
            }
        }

        stage('Test') {
            steps {
                bat 'dotnet test XUnit_Test/XUnit_Test.csproj --no-restore --verbosity normal /p:CollectCoverage=true /p:CoverletOutputFormat=opencover'
            }
        }

        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv('SonarQube') {
                    bat '''
                        dotnet tool install --global dotnet-sonarscanner || true
                        dotnet sonarscanner begin /k:"reservation-api" /d:sonar.host.url="http://localhost:9000/" /d:sonar.login="sqp_55efbeed057d640bc44e67cec936fbb9532cd530"  
                        dotnet build ctesp2425-final-gAf/ctesp2425-final-gAf.csproj --no-restore
                        dotnet sonarscanner end /d:sonar.login="sqp_8b3fe0b6a7aa8760fe8f98ea7191f30e96c2638a"
                    '''
                }
            }
        }

        stage('Docker Build') {
            steps {
                // Corrigido para especificar o caminho correto para o Dockerfile e contexto
                bat "docker build -t ${DOCKER_IMAGE} -f ctesp2425-final-gAf/Dockerfile ctesp2425-final-gAf"
            }
        }

        stage('Deploy') {
            steps {
                bat """
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