<?php

/**
 * Plugin Name: Events manager api - events
 * Add REST API support to an already registered post type.
 */
add_filter('register_post_type_args', 'my_post_type_args', 10, 2);

function my_post_type_args($args, $post_type) {

    if ('event' === $post_type) {
        $args['show_in_rest'] = true;

        // Optionally customize the rest_base or rest_controller_class
        $args['rest_base'] = 'events';
        $args['rest_controller_class'] = 'WP_REST_Posts_Controller';
    }

    return $args;
}

/**
 *  Expose the specific events data.
 */
add_action('init', 'register_new_meta');

function register_new_meta() {
     $args = array(
        'type' => 'string',
        'description' => '',
        'single' => true, // The meta key has a single value per object
        'auth_callback' => 'my_auth_callback',
        'show_in_rest' => true,
        'show_ui' => false,
        'show_in_menu' => false

    );

    function my_auth_callback($false, $meta_key, $post_id, $user_id, $cap, $caps) {
        if ($user_id === 1) {
            return true;
        } else {
            return false;
        }
    }

    $fields = ['_event_id', '_event_name', '_event_start_date', '_event_start_time', '_event_end_date', '_event_end_time', '_event_owner', '_event_status'];

    foreach ($fields as $key => $value)
    {
         register_meta('post', $value, $args );   
    }
}

/**
 * Enables the Excerpt meta box in Page edit screen, this allows the information to appear in REST.
 */
add_action('init', 'my_add_customfields_support_for_events');

function my_add_customfields_support_for_events() {
    add_post_type_support('event', 'custom-fields');
}