var grafoID;
var host = 'http://localhost:1234/';

$.ajaxSetup({
    beforeSend: function (request) {
        if (grafoID != undefined)
            request.setRequestHeader('X-Grafo-ID', grafoID);
        else if (this.url != host + 'api/grafo' && this.type != 'POST')
            console.log("Not authorized");
    },
    error: function (data, status, request) {
        console.log(new api.Response(data, status, request));
    }
});

var rest = {
    get: function (url, successCallback) {
        var apiResponse;

        $.ajax({
            url: host + url,
            type: 'GET',
            success: successCallback
        });

        return apiResponse;
    },
    post: function (url, data, successCallback) {
        var apiResponse;

        $.ajax({
            url: host + url,
            type: 'POST',
            data: data,
            success: successCallback
        });

        return apiResponse;
    },
    delete: function (url, data, successCallback) {
        var apiResponse;

        $.ajax({
            url: host + url,
            type: 'DELETE',
            data: data,
            success: successCallback
        });

        return apiResponse;
    }
}

var api = {
    Response: function (data, status, request) {
        this.data = data;
        this.status = status;
        this.request = request;
    },

    grafo: {
        get: function () {
            rest.get('api/grafo', function (data, status, request) {
                console.log(data);
            });
        },
        post: function (data) {
            rest.post('api/grafo', data, function (data, status, request) {
                grafoID = request.getResponseHeader('X-Grafo-ID')
            });
        },
        matriz: function () {
            rest.get('api/grafo/matriz', function (data, status, request) {
                console.log(data);
            });
        },
        grau: function () {
            rest.get('api/grafo/grau', function (data, status, request) {
                console.log(data);
            });
        },
        conexo: function () {
            rest.get('api/grafo/conexo', function (data, status, request) {
                console.log(data);
            });
        },
        euler: function () {
            rest.get('api/grafo/euler', function (data, status, request) {
                console.log(data);
            });
        }
    },

    vertice: {
        post: function (data) {
            rest.post('api/vertice', data, function (data, status, request) {
                console.log(data);
            });
        },
        delete: function(data) {
            rest.delete('api/vertice', data, function (data, status, request) {
                console.log(data);
            });
        },
        arestas: function(data) {
            rest.post('api/vertice/arestas', data, function (data, status, request) {
                console.log(data);
            });
        },
        adjacentes: function (data) {
            rest.post('api/vertice/adjacentes', data, function (data, status, request) {
                console.log(data);
            });
        },
        grau: function (data) {
            rest.post('api/vertice/grau', data, function (data, status, request) {
                console.log(data);
            });
        }
    },

    aresta: {
        post: function (data) {
            rest.post('api/aresta', data, function (data, status, request) {
                console.log(data);
            });
        },
        delete: function (data) {
            rest.delete('api/aresta', data, function (data, status, request) {
                console.log(data);
            });
        },
        lista: function (data) {
            rest.post('api/aresta/lista', data, function (data, status, request) {
                console.log(data);
            });
        }
    }
}