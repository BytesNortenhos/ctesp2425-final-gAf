pipeline {
    agent any

    environment {
        DOCKER_IMAGE = 'ctesp2425-final-gaf'
        DOCKER_HUB_REPO = 'robertovalentee/ctesp2425-final-gaf' // Substituir pelo reposit�rio Docker Hub
        SONAR_PROJECT_KEY = 'reservation-api'
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = 'true'
        SONAR_TOKEN = 'squ_541cd9e6b0a47ed44eec21c53da6831fde587044' // Hardcoded SonarQube
    }

    tools {
        dotnetsdk 'dotnet-sdk'
    }

    stages {
        // Est�gio 1: Instala��o do libicu (apenas para Unix)
        stage('Install libicu (Unix Only)') {
            when {
                expression { isUnix() } // Executa apenas em sistemas Unix-like
            }
            steps {
                sh '''
                    # Instala o libicu consoante a distribui��o Linux
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
                        echo "Gestor de pacotes n�o suportado. Por favor, instale o libicu manualmente."
                        exit 1
                    fi
                '''
            }
        }

        // Est�gio 2: Checkout do c�digo fonte
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        // Est�gio 3: Restauro das depend�ncias do projeto principal
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

        // Est�gio 4: Compila��o do projeto principal
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

        // Est�gio 5: Restauro das depend�ncias do projeto de testes XUnit
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

        // Est�gio 6: Execu��o dos testes XUnit
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

        // Est�gio 7: An�lise do c�digo com SonarQube
        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv('SonarQube') {
                    script {
                        if (isUnix()) {
                            sh '''
                                # Instala o dotnet-sonarscanner
                                dotnet tool install --global dotnet-sonarscanner || true

                                # Adiciona o diret�rio de ferramentas .NET ao PATH
                                export PATH="$PATH:/var/jenkins_home/.dotnet/tools"

                                # Verifica se o dotnet-sonarscanner est� dispon�vel
                                which dotnet-sonarscanner || echo "dotnet-sonarscanner not found"
                        
                                # Inicia a an�lise SonarQube com URL e autentica��o expl�citas
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

        // Est�gio 8: Constru��o da imagem Docker
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

        // Est�gio 9: Push da imagem Docker para o Docker Hub
        stage('Push Docker Image to Docker Hub') {
            steps {
                script {
                    docker.withServer('unix:///var/run/docker.sock') {
                        // Usa as configura��es do Jenkins para o URL do registo e credenciais
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

        // Est�gio 10: Implementa��o da imagem Docker a partir do Docker Hub
        stage('Deploy from Docker Hub') {
            steps {
                script {
                    docker.withServer('unix:///var/run/docker.sock') {
                        if (isUnix()) {
                            sh """
                                # Para e remove o contentor existente
                                docker stop ${DOCKER_IMAGE} || true
                                docker rm ${DOCKER_IMAGE} || true

                                # Faz pull da imagem mais recente do Docker Hub
                                docker pull ${DOCKER_HUB_REPO}:latest

                                # Executa o novo contentor
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

    // P�s-constru��o: Limpeza do espa�o de trabalho
    post {
        always {
            cleanWs() 
        }
    }
}