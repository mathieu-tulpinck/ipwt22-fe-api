<?php

/**
 * Plugin Name: Events manager api
 */

// Adds REST API support to an already registered post type.
add_filter('register_post_type_args', 'post_type_args', 10, 2);

function post_type_args($args, $post_type) {

    if ('event' === $post_type) {
        $args['show_in_rest'] = true;

        // Optionally customize the rest_base or rest_controller_class
        $args['rest_base'] = 'events';
        $args['rest_controller_class'] = 'WP_REST_Posts_Controller';
    }

    return $args;
}

/**
 *  Exposes the event and user metadata.
 */
add_action('init', 'register_new_meta');

function register_new_meta() {
     $event_args = array(
        'object_subtype' => 'event',
        'type' => 'string',
        'description' => '',
        'single' => true,// The meta key has a single value per object
        'auth_callback' => 'my_auth_callback',
        'show_in_rest' => true
    );

    $user_args = array(
        'type' => 'string',
        'description' => '',
        'single' => true,// The meta key has a single value per object
        'auth_callback' => null,
        'show_in_rest' => true
    );

    function my_auth_callback($false, $meta_key, $post_id, $user_id, $cap, $caps) {
        if ($user_id === 1) {
            return true;
        } else {
            return false;
        }
    }

    $event_meta_keys = ['_event_id', '_event_start', '_event_end'];

    foreach ($event_meta_keys as $meta_key)
    {
         register_meta('post', $meta_key, $event_args);   
    }

    register_meta('user', 'vat_nr', $user_args);
}


// Enables custom-fields support for event type. 
add_action('init', 'add_customfields_support_for_events');

function add_customfields_support_for_events() {
    add_post_type_support('event', 'custom-fields');
}