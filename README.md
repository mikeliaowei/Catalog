# Catalog
.Net 5 Rest API Tutorial demo

1. How deploy k8s
    kubectl config current-context

    kubectl create secret generic catalog-secretes --from-literal=mongodb-password='Pass#word1'

    cd .\kubernetes

    kubectl apply -f .\catalog.yaml
    
    kubectl apply -f .\mongodb.yaml 

    kubectl get deployments

    kubectl get pods
    kubectl get statefulsets
