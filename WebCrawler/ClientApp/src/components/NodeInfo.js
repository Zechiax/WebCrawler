import React, { Component } from "react";
import Modal from "react-bootstrap/Modal";
import ListGroup from "react-bootstrap/ListGroup";
import { Label } from "reactstrap";
import Button from "react-bootstrap/Button";
import { WebsiteRecordInfoModal } from "./WebsiteRecordInfoModal";

export class NodeInfo extends Component {
  constructor(props) {
    super(props);

    this.state = {
      createNewWebsiteRecord: {
        show: false,
      },
    };
  }

  render() {
    return (
      <>
        <Modal
          show={this.props.show}
          onHide={this.props.onHide}
          size="lg"
          centered
        >
          <Modal.Header closeButton>Node Info</Modal.Header>
          <ListGroup
            style={{
              margin: 10,
            }}
          >
            <Label>Title:</Label>
            <ListGroup.Item> {this.props.title}</ListGroup.Item>
            <Label style={{ marginTop: "10px" }}>Url:</Label>
            <ListGroup.Item>{this.props.url}</ListGroup.Item>
            <Label style={{ marginTop: "10px" }}>Crawl time:</Label>
            <ListGroup.Item>{this.props.crawlTime}</ListGroup.Item>
            <Label style={{ marginTop: "10px" }}>Crawled by website records:</Label>
            <ListGroup>
              {this.props.records.map((record, i) => {
                return (
                  // TODO: add arrow that will reveal more info about website record - component from home.js table
                  // HOW: we have id, so just fetch the record from server and bind it to the props
                  <ListGroup.Item
                    key={i}
                    variant="light"
                  >{`${record.label} (id: ${record.id})`}</ListGroup.Item>
                );
              })}
            </ListGroup>
          </ListGroup>
          <div
            style={{
                margin: 10,
                display: "flex",
                justifyContent: "flex-end"
            }}
          >
            <Button
              onClick={() => {
                this.setState({
                  createNewWebsiteRecord: {
                    show: true,
                  },
                });
              }}
              variant="primary"
            >
              Create new website record from this node
            </Button>
          </div>
        </Modal>

        <WebsiteRecordInfoModal
            show={this.state.createNewWebsiteRecord.show}
            urlPresetValue={this.props.url}
            labelPresetValue={this.props.title}
            isActivePresetValue={true}
            periodicityPresetValue={10}
            regexPresetValue={".*"}
            isEditing={false}
            onClose={() =>
                this.setState({
                    createNewWebsiteRecord: {
                        show: false,
                    },
                })
            }
        />
      </>
    );
  }
}
