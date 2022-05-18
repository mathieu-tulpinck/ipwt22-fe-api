<?php

/**
 * Plugin Name: Events manager hooks
 */

add_filter('em_event_save', 'notify_event', 10, 2);

function notify_event($result, $event) {
    $post = get_post($event->post_id);
    $created  = strtotime($post->post_date_gmt);
    $modified = strtotime($post->post_modified_gmt);
    $event_start = strtotime($event->event_start_date . " " . $event->event_start_time);
    $event_end = strtotime($event->event_start_date . " " . $event->event_start_time);
    // Event created.
    if ($created == $modified && $event->event_status == 1) {
        $status = true;
        $method = 'CREATE';
        $attributes = json_decode(json_encode($event), true);
        send_event_payload($event->event_id, $attributes, $event_start, $event_end, $status, $method);
    } 
    // Event placed in draft.
    else if (is_null($event->event_status)) {
        $status = false;
        $method = 'UPDATE';
        $attributes = array('status' => $event->event_status);
        send_event_payload($event->event_id, $attributes, $event_start, $event_end, $status, $method);
    }

    return $result;
}

// Event updated. Only modified attributes are sent to Producer.
add_action('em_event_save_pre', 'notify_event_update');

function notify_event_update($event) {
    $post = get_post($event->post_id);
    $created  = strtotime($post->post_date_gmt);
    $modified = strtotime($post->post_modified_gmt);
    $event_start = strtotime($event->event_start_date . " " . $event->event_start_time);
    $event_end = strtotime($event->event_start_date . " " . $event->event_start_time);
    
    if ($created != $modified && $event->event_status == 1) {
        $event_id = $event->event_id;
        global $wpdb;
        $old = $wpdb->get_row($wpdb->prepare("SELECT * FROM " . EM_EVENTS_TABLE . ' WHERE event_id=%s', $event_id), ARRAY_A);
        $master = array('event_id' => 0, 'event_owner' => 0, 'event_status' => 0, 'event_name' => 0, 'event_start' => 0, 'event_end' => 0);
        $old_intersect = array_intersect_key($old, $master);
        $old_intersect["event_start"] = strtotime($old_intersect["event_start"]);
        $old_intersect["event_end"] = strtotime($old_intersect["event_end"]);
        $event_array = json_decode(json_encode($event), true); // Convert to array.
        $event_array_intersect = array_intersect_key($event_array, $master);
        $event_array_intersect["event_start"] = strtotime($event->event_start_date." ".$event->event_start_time);
        $event_array_intersect["event_end"] = strtotime($event->event_end_date." ".$event->event_end_time);
        $diff = array_diff($old_intersect, $event_array_intersect);
        if (!empty($diff)) {
            $status = true;
            $method = 'UPDATE';
            $attributes = array();
            foreach (array_keys($diff) as $key) {
                $attributes[$key] = $event_array_intersect[$key];
            };
            send_event_payload($event_id, $attributes, $event_start, $event_end, $status, $method);
        }
    } 
}

// Event placed in trash.
add_action('wp_trash_post', 'notify_event_delete');

function notify_event_delete($post_id) {
    global $wpdb;
    $event = $wpdb->get_row($wpdb->prepare("SELECT * FROM " . EM_EVENTS_TABLE . ' WHERE post_id=%s', $post_id), OBJECT);
    $headers = array(
        'Content-Type' => 'application/json'
    );
    $args = array(
        'method' => 'DELETE',
        'headers' => $headers     
    );
    wp_remote_request(PRODUCER_URL . "/api/events/{$event->id}", $args);
}

function send_event_payload($event_id, $attributes, $event_start, $event_end, $status, $method) {
    $headers = array(
        'Content-Type' => 'application/json'
    );

    if ($method == 'CREATE') {
        $body = array(
            'id' => $attributes['event_id'],
            'owner' => $attributes['event_owner'], // OrganiserUuid
            'status' => $status, // IsActive
            'name' => $attributes['event_name'], // Title
            'start' => $event_start, // StartDateUTC
            'end' => $event_end, // EndDateUTC
        );

        $body = wp_json_encode($body);

        $args = array(
            'headers' => $headers,     
            'body' => $body,
            'data_format' => 'body'
        );

        wp_remote_post(PRODUCER_URL . "/api/events", $args);
    } else if ($method == 'UPDATE') {
        $body = wp_json_encode($attributes);

        $args = array(
            'method' => 'PATCH',
            'headers' => $headers,     
            'body' => $body,
            'data_format' => 'body'
        );
        
        wp_remote_request(PRODUCER_URL . "/api/events/{$event_id}", $args);
    }
}

// 0 => 'Pending',
// 1 => Approved',
// 2 => 'Rejected',
// 3 => 'Cancelled',
// 4 => 'Awaiting Online Payment',
// 5 => 'Awaiting Payment'
// Booking created.
add_filter('em_booking_save', 'notify_booking_created', 10, 3);

function notify_booking_created ($count, $booking, $update) {
    $method = 'CREATE';
    $attributes = json_decode(json_encode($booking), true); // Convert to array.
    send_booking_payload($booking->id, $booking, $method);

    return $count;
}

// Booking status updated.
add_filter('em_booking_set_status','notify_booking_status', 10, 2);

function notify_booking_status($result, $booking) {
    $method = 'UPDATE';
    $attributes = array('bookingStatus' => $booking->booking_status);
    send_booking_payload($booking->id, $attributes, $method);

    return $result;
}

// Booking deleted.
add_filter('em_booking_delete', 'notify_booking_deleted', 10, 2);

function notify_booking_deleted ($result, $booking) {
    $headers = array(
        'Content-Type' => 'application/json'
    );
    $args = array(
        'method' => 'DELETE',
        'headers' => $headers     
    );
    wp_remote_request(PRODUCER_URL . "/api/bookings/{$booking->id}", $args);

    return $result;
}

function send_booking_payload($booking_id, $attributes, $method) {
    $headers = array(
        'Content-Type' => 'application/json'
    );
    
    if ($method == 'CREATE') {
        $body = array(
            'id' => $attributes['booking_id'],
            'eventId' => $attributes['event_id'],
            'personId' => $attributes['person_id'], // Name, LastName, Email, VatNumber
            'bookingSpaces' => $attributes['booking_spaces'],
            'bookingStatus' => $attributes['booking_status'], // InvitationStatus
        );  

        $body = wp_json_encode($body);

        $args = array(
            'headers' => $headers,     
            'body' => $body
        );

        wp_remote_post(PRODUCER_URL . "/api/bookings", $args);
    } else if ($method == 'UPDATE') {
        $body = wp_json_encode($attributes);

        $args = array(
            'method' => 'PATCH',
            'headers' => $headers,     
            'body' => $body,
            'data_format' => 'body'
        );
        
        wp_remote_request(PRODUCER_URL . "/api/bookings/{$booking_id}", $args);
    }



    
}

