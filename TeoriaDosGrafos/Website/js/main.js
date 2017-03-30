var submitInfo;
var page;
var message_max = 25;

$(document).ready(function(){

	changePage = function(id) {
		if (id == page) return;

		$('.active').hide();
		$('.active').removeClass('active');

		var toActivate = (id != null) ? id : page;
		page = toActivate;

		$(toActivate).show();

	    var totop = setInterval(function () {
	      $(".pages").animate({scrollTop: 0}, 0);
	    }, 1);

	  setTimeout(function () {
	      $(toActivate).addClass('active');
	      setTimeout(function () {
	        clearInterval(totop);
	      }, 1000);
	    }, 100);
	}

    changePage('#novoGrafo');
	$('.page-button').click(function(){changePage($(this).data('target-page'))})

    $.material.init();

    $(document).on('click', 'div[class^="item-"] .btn-add', function () {
        var $item = $(this).parents('div[class^="item-"]');
        $item.after($item.clone().find('input[type!="hidden"]').val('').end());
    });

    $(document).on('click', 'div[class^="item-"] .btn-remove', function () {
        var $item = $(this).parents('div[class^="item-"]');
        $item.remove();
    });

    $('input[type=radio][name=leitura]').change(function () {
        if (this.value == 'arquivo') {
            $('.inserir-dados').hide();
            $('.arquivo').show();
            $('.arquivo').find('input').prop('required', true);
        }
        else if (this.value == 'inserir') {
            $('.arquivo').hide();
            $('.inserir-dados').show();
            $('.inserir-dados').find('input').prop('required', true);
        }
    });

})

var getQueryString = function (form) {
    var inputs = $(form).serializeArray();
    var queryString = '';

    if (inputs[0].value == 'arquivo') {

        queryString = false;

        var fileInput = $(form).find('#json')[0];

        var file = fileInput.files[0];
        var fr = new FileReader();
        fr.onload = function () {
            api.grafo.post('json=' + fr.result);
        };
        fr.readAsText(file);
    }

    else {
        var i = 1;
        while (i < inputs.length) {
            if (i != 1)
                queryString += '&';
            queryString += inputs[i].value;
            queryString += '={';

            var j = i + 1;
            while ((j < inputs.length) && inputs[j].name != 'tipo') {
                queryString += inputs[j].name;
                queryString += ':\'';
                queryString += inputs[j].value;
                queryString += '\'';

                if ((j + 1 < inputs.length) && (inputs[j + 1].name != 'tipo'))
                    queryString += ', ';

                ++j;
            }

            queryString += '}';
            i = j;
        }
    }
    
    return queryString;
}

var alerta = function (sucesso) {

    if (sucesso != undefined) {
        if (sucesso) {
            $('#error-alert').hide();
            $('#success-alert').fadeIn(300).delay(2000).fadeOut(300);
        }
        else {
            $('#success-alert').hide();
            $('#error-alert').fadeIn(300).delay(2000).fadeOut(300);
        }
    }
    else {
        $('#success-alert').fadeOut();
        $('#error-alert').fadeOut();
    }
}

var novoGrafo = function () {
    var form = '#novoGrafo form';
    var queryString = getQueryString(form);

    if (queryString != false)
        api.grafo.post(queryString);
    return false;
}

var novoVertice = function () {
    var formData = $('#novoVertice form').serializeArray();
    var queryString = 'id=' + formData[0].value + '&nome=' + formData[1].value;

    api.vertice.post(queryString);
    return false;
}

var apagaVertice = function () {
    var formData = $('#apagaVertice form').serializeArray();
    var queryString = 'id=' + formData[0].value;

    api.vertice.delete(queryString);
    return false;
}

var arestasOfVertice = function () {
    var formData = $('#arestasVertice form').serializeArray();
    var queryString = 'id=' + formData[0].value;

    api.vertice.arestas(queryString);
    return false;
}

var grauVertice = function () {
    $('#grauVertice').find('.item-grau').hide();
    var formData = $('#grauVertice form').serializeArray();
    var queryString = 'id=' + formData[0].value;

    api.vertice.grau(queryString);
    return false;
}


var verticesAdjacentes = function () {
    $('#verticesAdjacentes').find('pre').hide();
    var formData = $('#verticesAdjacentes form').serializeArray();
    var queryString = 'id=' + formData[0].value;

    api.vertice.adjacentes(queryString);
    return false;
}

var novaAresta = function () {
    var formData = $('#novaAresta form').serializeArray();
    var queryString = 'origem=' + formData[0].value + '&destino=' + formData[1].value + '&peso=' + formData[1].value;

    api.aresta.post(queryString);
    return false;
}

var apagaAresta = function() {
    var formData = $('#apagaAresta form').serializeArray();
    var queryString = 'vertice1=' + formData[0].value + '&vertice2=' + formData[1].value;

    api.aresta.delete(queryString);
    return false;
}

var arestasEntreVertices = function () {
    $('#arestasEntreVertices').find('pre').hide();
    var formData = $('#arestasEntreVertices form').serializeArray();
    var queryString = 'vertice1=' + formData[0].value + '&vertice2=' + formData[1].value;

    api.aresta.lista(queryString);
    return false;
}


var svg = d3.select("svg"),
    width = +svg.attr("width"),
    height = +svg.attr("height");

var color = d3.scaleOrdinal(d3.schemeCategory20);

var simulation = d3.forceSimulation()
    .force("link", d3.forceLink().id(function (d) { return d.id; }))
    .force("charge", d3.forceManyBody())
    .force("center", d3.forceCenter(width / 2, height / 2));

d3.json("Grafos.json", function (error, graph) {
    if (error) throw error;

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
        .text(function (d) { return d.nome; });

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
});

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
