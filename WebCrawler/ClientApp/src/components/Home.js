import React, { Component } from "react";
import Container from "react-bootstrap/Container";
import Button from "react-bootstrap/Button";
import WebsiteRecordInfoModal from "./WebsiteRecordInfoModal";
import PaginatedView from "./PaginatedView";
import Modal from "react-bootstrap/Modal";
import Spinner from 'react-bootstrap/Spinner';

export class Home extends Component {
  static displayName = Home.name;

  constructor(props) {
    super(props);

    this.state = {
      isCreateWebsiteRecordModalShown: false,
      dataUpdatehandler: null,
      loading: false,
      modalWindowContext: {
        id: 0,
        name: "",
        isActive: true,
        tags: [],
        periodicity: 10,
        regexPattern: ".*",
        entryUrl: "",
        isEditing: false,
      },
    };
  }

  onCreateWebsiteRecordModalClose = () => {
    this.setState({ isCreateWebsiteRecordModalShown: false });
  };

  handleEditWebsiteRecord = (context) => {
    this.setState({
      isCreateWebsiteRecordModalShown: true,
      modalWindowContext: {
        ...context,
        tags: [...context.tags],
      },
    });
  };

  registerDataUpdateHandler = (handler) => {
    this.setState({ dataUpdatehandler: handler });
    };

    loadingStart = () => {
        this.setState({ loading: true });
        console.log("loading start");
    };

    loadingStop = () => {
        this.setState({ loading: false });
        console.log("loading stop");
    };

  render() {
    return (
      <>
        <Container
          fluid="md"
          style={{ display: "flex", flexDirection: "column" }}
        >
          <div style={{ flex: "1", marginBottom: "1rem" }}>
            <PaginatedView
              showEditModalWindow={this.handleEditWebsiteRecord}
              registerDataUpdateHandler={this.registerDataUpdateHandler}
              loadingStart={this.loadingStart}
              loadingStop={this.loadingStop}
            />
          </div>

          <div style={{ display: "flex", justifyContent: "flex-end" }}>
            <Button
              variant="primary"
              onClick={() =>
                  this.setState({
                      isCreateWebsiteRecordModalShown: true,
                      modalWindowContext: {
                          id: 0,
                          name: "",
                          isActive: true,
                          tags: [],
                          periodicity: 10,
                          regexPattern: ".*",
                          entryUrl: "",
                          isEditing: false,
                        },
                  })

              }
            >
              Create new website record
            </Button>
          </div>
        </Container>

        <Modal
          show={this.state.loading}
          centered
          size="sm"
        >
          <Modal.Body>
            <div
                style={{ 
                    display: "flex",
                    justifyContent: "center",
                }}>
                <Spinner
                    animation="border"
                    role="status"
                />
                <span style={{ marginLeft: "0.5rem", fontWeight: "bold" }}>
                    Loading...
                </span>
            </div>
          </Modal.Body>
        </Modal>

        <WebsiteRecordInfoModal
          show={this.state.isCreateWebsiteRecordModalShown}
          urlPresetValue={this.state.modalWindowContext.entryUrl}
          labelPresetValue={this.state.modalWindowContext.name}
          isActivePresetValue={this.state.modalWindowContext.isActive}
          tagsPresetValue={[...this.state.modalWindowContext.tags]}
          periodicityPresetValue={this.state.modalWindowContext.periodicity}
          regexPresetValue={this.state.modalWindowContext.regexPattern}
          recordId={this.state.modalWindowContext.id}
          isEditing={this.state.modalWindowContext.isEditing}
          passCreatedRecordId={this.state.dataUpdatehandler}
          onClose={this.onCreateWebsiteRecordModalClose}
        />
      </>
    );
  }
}
