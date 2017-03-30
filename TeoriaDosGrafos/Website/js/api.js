var grafoID;
var host = 'http://localhost:1234/';


$.ajaxSetup({
    beforeSend: function (xhr) {
        if (grafoID != undefined)
            xhr.setRequestHeader('X-Grafo-ID', grafoID);
        else if (this.url != host + 'api/grafo' && this.type != 'POST')
            console.log("Not authorized");
    },
    error: function (data, status, request) {
        console.log(request);
    }
});

var api = {
    grafo: {
        get: function () {
            $.ajax({
                url: host + 'api/grafo',
                type: 'GET',
                success: function (data) {
                    console.log(data);
                }
            });

        },
        post: function (data) {
            $.ajax({
                url: host + 'api/grafo',
                type: 'POST',
                data: data,
                success: function (data, status, request) {
                    grafoID = request.getResponseHeader('X-Grafo-ID');
                }
            });

        },
    },

    vertice: {
    },

    aresta: {

    }
}
