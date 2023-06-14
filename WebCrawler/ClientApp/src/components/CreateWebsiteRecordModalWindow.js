import React, { Component } from "react";
import Modal from "react-bootstrap/Modal";
import Button from "react-bootstrap/Button";

export class CreateWebsiteRecordModalWindow extends Component {
  constructor(props) {
    super(props);
  }

  render() {
    return (
      <Modal show={this.props.show} onHide={this.props.handleClose}>
        <Modal.Header closeButton>
          <Modal.Title>Website Record</Modal.Title>
        </Modal.Header>
        <Modal.Body>Woohoo, you are reading this text in a modal!</Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={this.props.handleClose}>
            Close
          </Button>
          <Button variant="success" onClick={this.props.handleClose}>
            Create
          </Button>
        </Modal.Footer>
      </Modal>
    );
  }
}
