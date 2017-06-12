var grafoID;
var host = 'http://localhost:1234/';

$.ajaxSetup({
    beforeSend: function (request) {
        if (grafoID !== undefined)
            request.setRequestHeader('X-Grafo-ID', grafoID);
        else if (this.url !== host + 'api/grafo' && this.type !== 'POST')
            console.log("Not authorized");
    },
    error: function (data, status, request) {
        alerta(false);
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
};

var api = {
    Response: function (data, status, request) {
        this.data = data;
        this.status = status;
        this.request = request;
    },

    grafo: {
        get: function () {
            rest.get('api/grafo', function (data, status, request) {
                showJSON('#lerGrafo', data);
                changePage('#lerGrafo');
            });
        },
        post: function (data) {
            rest.post('api/grafo', data, function (data, status, request) {
                grafoID = request.getResponseHeader('X-Grafo-ID');
                alerta(true);
                ativarBotoes(true);
            });
        },
        ver: function () {
            rest.get('api/grafo', function (graph, status, request) {
                $('svg').text("");

                var svg = d3.select("svg"),
                    width = +svg.attr("width"),
                    height = +svg.attr("height");

                var color = d3.scaleOrdinal(d3.schemeCategory20);

                var simulation = d3.forceSimulation()
                    .force("link", d3.forceLink().id(function (d) { return d.id; }))
                    .force("charge", d3.forceManyBody())
                    .force("center", d3.forceCenter(width / 2, height / 2));

                function dragstarted(d) {
                    if (!d3.event.active) simulation.alphaTarget(0.3).restart();
                    d.fx = d.x;
                    d.fy = d.y;
                }

                function dragged(d) {
                    d.fx = d3.event.x;
                    d.fy = d3.event.y;
                }

                function dragended(d) {
                    if (!d3.event.active) simulation.alphaTarget(0);
                    d.fx = null;
                    d.fy = null;
                }

                var link = svg.append("g")
                    .attr("class", "links")
                    .selectAll("line")
                    .data(graph.Arestas)
                    .enter().append("line")
                    .attr("stroke-width", function (d) { return Math.sqrt(d.peso); });

                var node = svg.append("g")
                    .attr("class", "nodes")
                    .selectAll("circle")
                    .data(graph.Vertices)
                    .enter().append("circle")
                    .attr("r", 5)
                    .attr("fill", function (d) { return color(d.id); })
                    .call(d3.drag()
                        .on("start", dragstarted)
                        .on("drag", dragged)
                        .on("end", dragended));

                node.append("title")
                    .text(function (d) { return d.id + d.nome !== undefined ? ' (' + d.nome + ')' : ''; });

                simulation
                    .nodes(graph.Vertices)
                    .on("tick", ticked);

                simulation.force("link")
                    .links(graph.Arestas);

                function ticked() {
                    link
                        .attr("x1", function (d) { return d.source.x; })
                        .attr("y1", function (d) { return d.source.y; })
                        .attr("x2", function (d) { return d.target.x; })
                        .attr("y2", function (d) { return d.target.y; });

                    node
                        .attr("cx", function (d) { return d.x; })
                        .attr("cy", function (d) { return d.y; });
                }

                changePage('#verGrafo');
            });
        },
        matriz: function () {
            rest.get('api/grafo/matriz', function (data, status, request) {
                $('#matrizDeAdjacencia').find('.placeholder').html(data);
                changePage('#matrizDeAdjacencia');
            });
        },
        matrizAcessibilidade: function () {
            rest.get('api/grafo/matriz/acessibilidade', function (data, status, request) {
                $('#matrizAcessibilidade').find('.placeholder').html(data);
                changePage('#matrizAcessibilidade');
            });
        },
        menorCaminhoFloydWarshall: function () {
            rest.get('api/grafo/menorCaminhoFloydWarshall', function (data, status, request) {
                $('#menorCaminhoFloydWarshall').find('.placeholder').html(data);
                changePage('#menorCaminhoFloydWarshall');
            });
        },
        menorCaminhoDijkstra: function (data) {
            rest.post('api/grafo/menorCaminhoDijkstra', data, function (data, status, request) {
                $('#menorCaminhoDijkstra').find('.placeholder').html(data).show();
                //$('#menorCaminhoDijkstra').find('form').hide();
            });
        },
        menorCaminhoBellmanFord: function (data) {
            rest.post('api/grafo/menorCaminhoBellmanFord', data, function (data, status, request) {
                $('#menorCaminhoBellmanFord').find('.placeholder').html(data).show();
                // $('#menorCaminhoBellmanFord').find('form').hide();
            });
        },
        getBenchmark: function (data) {
            rest.post('api/grafo/benchmark', data, function (data, status, request) {
                showJSON('#benchmark', data);
            });
        },
        grau: function () {
            rest.get('api/grafo/grau', function (data, status, request) {
                $('#grau-maximo').find('h2').text(data[2].NumGrau);
                $('#grau-maximo').find('h4').text(' (Vértice: ' + data[2].Vertice.id + ' - ' + data[2].Vertice.nome + ')');

                $('#grau-medio').find('h2').text(data[1].NumGrau);

                $('#grau-minimo').find('h2').text(data[0].NumGrau);
                $('#grau-minimo').find('h4').text(' (Vértice: ' + data[0].Vertice.id + ' - ' + data[0].Vertice.nome + ')');
                changePage('#verGrau');
            });
        },
        conexo: function () {
            rest.get('api/grafo/conexo', function (data, status, request) {
                var icon = data ? 'check_circle' : 'cancel';
                var color = data ? 'green' : 'red';

                $('#is-conexo-icon').text(icon);
                $('#is-conexo-icon').css('color', color);
                changePage('#isConexo');
            });
        },
        euler: function () {
            rest.get('api/grafo/euler', function (data, status, request) {
                var icon = data ? 'check_circle' : 'cancel';
                var color = data ? 'green' : 'red';

                $('#possui-euler-icon').text(icon);
                $('#possui-euler-icon').css('color', color);
                changePage('#possuiEuler');
            });
        }
    },

    vertice: {
        post: function (data) {
            rest.post('api/vertice', data, function (data, status, request) {
                alerta(true);
            });
        },
        delete: function (data) {
            rest.delete('api/vertice', data, function (data, status, request) {
                alerta(true);
            });
        },
        arestas: function (data) {
            rest.post('api/vertice/arestas', data, function (data, status, request) {
                showJSON('#arestasVertice', data);
            });
        },
        adjacentes: function (data) {
            rest.post('api/vertice/adjacentes', data, function (data, status, request) {
                showJSON('#verticesAdjacentes', data);
            });
        },
        grau: function (data) {
            rest.post('api/vertice/grau', data, function (data, status, request) {
                $('#grauVertice').find('.item-grau').find('h2').text(data);
                $('#grauVertice').find('.item-grau').show();
            });
        }
    },

    aresta: {
        post: function (data) {
            rest.post('api/aresta', data, function (data, status, request) {
                alerta(true);
            });
        },
        delete: function (data) {
            rest.delete('api/aresta', data, function (data, status, request) {
                alerta(true);
            });
        },
        lista: function (data) {
            rest.post('api/aresta/lista', data, function (data, status, request) {
                showJSON('#arestasEntreVertices', data);
            });
        }
    }
};