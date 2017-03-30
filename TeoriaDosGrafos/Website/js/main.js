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

})

var getQueryString = function (form) {
    var inputs = $(form).serializeArray();
    var queryString = '';

    var i = 0;
    while (i < inputs.length) {
        if (i != 0)
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
    
    return queryString;
}

var alerta = function (sucesso) {

    if (sucesso != undefined) {
        if (sucesso) {
            $('#error-alert').hide();
            $('#success-alert').fadeIn();
        }
        else {
            $('#success-alert').hide();
            $('#error-alert').fadeIn();
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