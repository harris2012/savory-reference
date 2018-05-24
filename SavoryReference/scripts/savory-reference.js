function WelcomeController($scope) {

    $('#jstree_demo_div').jstree({
        "core": {
            "animation": 0,
            "check_callback": true,
            "themes": { "stripes": true },
            data: function (item, callback) {
                $.post('/api/node/items', { id: item.id }, function (response) {
                    callback.call(this, response.items);
                })
            }
        },
        "types": {
            "#": {
                "max_children": 1,
                "max_depth": 4,
                "valid_children": ["root"]
            },
            "class": {
                "icon": "/images/reference_class.png",
                "valid_children": ["default"]
            },
            "interface": {
                "icon": "/images/reference_interface.png",
                "valid_children": ["default"]
            },
            "enum": {
                "icon": "/images/reference_enum.png",
                "valid_children": ["default"]
            },
            "method": {
                "icon": "/images/reference_method.png",
                "valid_children": ["default"]
            },
            "property": {
                "icon": "/images/reference_property.png",
                "valid_children": ["default"]
            },
            "property_inherit": {
                "icon": "/images/reference_property_inherit.png",
                "valid_children": ["default"]
            },
            "property_static": {
                "icon": "/images/reference_property_static.png",
                "valid_children": ["default"]
            },
            "default": {
                "valid_children": ["default", "file"]
            },
            "file": {
                "icon": "glyphicon glyphicon-file",
                "valid_children": []
            }
        },
        "plugins": [
           "search", "types", "wholerow"
        ]
    });

    var to = false;
    $('#plugins4_q').keyup(function () {
        if (to) {
            clearTimeout(to);
        }
        to = setTimeout(function () {
            var v = $('#plugins4_q').val();
            $('#jstree_demo_div').jstree(true).search(v);
        }, 250);
    });

    $('#jstree_demo_div').on("changed.jstree", function (e, data) {

        if (!data || !data.selected || !data.selected.length) {
            return;
        }

        $.post('/api/node/items', { id: data.selected[0] }, function (response) {

            $('#detail').text(JSON.stringify(response.items, null, 2));

            //console.log(response);
        });
    });

    $('.leftMenu').resizable({
        handles: "e, w",
        minWidth: 300,
        maxWidth: 600,
        resize: function (event, ui) {
            $('.rightContent').css('margin-left', ui.size.width)
        }
    });
}
var route = function ($stateProvider, $urlRouterProvider) {

    $urlRouterProvider.when('', '/welcome').when('/', '/welcome');

    $stateProvider.state('app', {
        url: '/'
    });

    $stateProvider.state('app.welcome', {
        url: 'welcome',
        templateUrl: 'scripts/view/view_welcome.html?v=' + window.version,
        controller: WelcomeController
    });
}
var app = angular.module('app', ['ngResource', 'ui.router']);

app.config(route);